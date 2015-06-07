using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using System.IO;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Serialization;
using Weapons;
using TegForExtensions;
using Interfaces;
using AboutInterfaceComparers;
using Microsoft.Win32;
using HashInterface;


namespace OOPlaba4
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string pluginPath = System.IO.Path.Combine(
                                                Directory.GetCurrentDirectory(),
                                                "Plugins");
        static JsonSerializer jsonSerializer = new JsonSerializer();
        List<string> classesNames = new List<string>();
        List<Type> dllTypes = new List<Type>();
        List<Type> otherDllTypes = new List<Type>();
        List<IExt> plugins = new List<IExt>();
        List<IComparer> comparerPlugins = new List<IComparer>();
        List<Adapter> hashCreatorsPlugins = new List<Adapter>();
        string sourcePath = Environment.CurrentDirectory;
        ObservableCollection<Weapon> weapons = new ObservableCollection<Weapon>();
        ObservableCollection<Weapon> weapons1 = new ObservableCollection<Weapon>();
        ObservableCollection<Weapon> weapons2 = new ObservableCollection<Weapon>();
        ObservableCollection<TegForExtensions.Extension> extensions = new ObservableCollection<TegForExtensions.Extension>();
        Type[] hashTypes;
        public MainWindow()
        {   
            InitializeComponent();
        }

        class Wrapper
        {
            public ObservableCollection<Weapon> Weapons { get; set; }
        }
        //find all plugins and create there instances
        private void RefreshPlugins()
        {
            plugins.Clear();
            DirectoryInfo pluginDirectory = new DirectoryInfo(pluginPath);
            if (!pluginDirectory.Exists)
                pluginDirectory.Create();                 
            var pluginFiles = Directory.GetFiles(pluginPath, "*.dll");
            var hierarchyPluginFiles = Directory.GetFiles(sourcePath, "*.dll");
            foreach (var file in pluginFiles)
            {
                
                Assembly asm = Assembly.LoadFrom(file);
                
                var types = asm.GetTypes().
                                Where(t => t.GetInterfaces().
                                Where(i => i.FullName == typeof(IExt).FullName).Any());
                var typesChkCompare = asm.GetTypes().
                                Where(t => t.GetInterfaces().
                                Where(i => i.FullName == typeof(IComparer).FullName).Any());
                
                foreach (var type in types)
                {
                    var plugin = asm.CreateInstance(type.FullName) as IExt;
                    plugins.Add(plugin);
                    listOfExtensions.Items.Add(plugin);
                }
                foreach (var type in typesChkCompare)
                {
                    var plugin = asm.CreateInstance(type.FullName) as IComparer;
                    comparerPlugins.Add(plugin);
                    listOfExtensions.Items.Add(plugin);
                }
            }
            foreach (var file in hierarchyPluginFiles)
            {
                Assembly asmClasses = Assembly.LoadFrom(file);
                var types = asmClasses.GetTypes().Where(type => type.IsSubclassOf(typeof(Weapon)));
                foreach (var type in types)
                {
                    var plugin = asmClasses.CreateInstance(type.FullName) as Weapon;
                    weapons1.Add(plugin);
                }
            }
            Weapon weapon = new Weapon();
            weapons1.Add(weapon);
        }
        //call methods of saving CRC 
        private void ActivateSaveCRCPlugins()
        {
            foreach (var plugin in plugins)
                plugin.saveChkSum(weapons, "test.txt");
            if(hashCreatorsPlugins != null)
                foreach (var plugin in hashCreatorsPlugins)
                    plugin.saveChkSum(weapons, "test2.txt");
            
        }
        //call methods of checking CRC
        private void ActivateCompareCRCPlugins()
        {
            
            foreach (var plugin in comparerPlugins)
                MessageBox.Show("CRC is Correct? " + plugin.ChkSumCompare("test.txt").ToString());
            
        }

       

        
        //displays all properties of classes in plugins hierarchy
        private void ShowAllProperties(ListBox list)
        {
            int i = 0;
            Type type = list.SelectedItem.GetType();
            Fields.Children.Clear();
            foreach (PropertyInfo property in type.GetProperties())
            {
                TextBox textBox = new TextBox();
                Label label = new Label();
                Binding binding = new Binding();
                binding.Source = list.SelectedItem;
                binding.Path = new PropertyPath(property.Name);
                binding.Mode = BindingMode.TwoWay;
                
                label.Content = property.Name;
                textBox.SetBinding(TextBox.TextProperty, binding);
                Grid.SetColumn(label, 0);
                Grid.SetColumn(textBox, 1);
                Grid.SetRow(label, i);
                Grid.SetRow(textBox, i);

                var rowDefinition = new RowDefinition { Height = GridLength.Auto };
                Fields.RowDefinitions.Add(rowDefinition);
                Fields.Children.Add(textBox);
                Fields.Children.Add(label);
                i++;
            }
        }

        private void GetListOfWeapons_Click(object sender, RoutedEventArgs e)
        {
            
            
            RefreshPlugins();           
            WeaponsTypes.ItemsSource = weapons1;
            WeaponsList.ItemsSource = weapons;
            
        }


        

        private void Serialization_Click(object sender, RoutedEventArgs e)
        {
            File.Delete("test.txt");
            File.Delete("test2.txt");
            ActivateSaveCRCPlugins();
            FileStream stream;
            stream = File.Open("BsonFile", FileMode.OpenOrCreate, FileAccess.Write);

            using (BsonWriter bsonWriter = new BsonWriter(stream))
            {
                jsonSerializer.TypeNameHandling = TypeNameHandling.All;
                var obj = new Wrapper { Weapons = weapons };                
                jsonSerializer.Serialize(bsonWriter, obj);
            }
            stream.Close();
            

        }

        private void Deserialization_Click(object sender, RoutedEventArgs e)
        {
            FileStream stream;
            stream = File.Open("BsonFile", FileMode.Open, FileAccess.Read);

            using (BsonReader bsonReader = new BsonReader(stream))
            {
                jsonSerializer.TypeNameHandling = TypeNameHandling.All;
                var obj = new Wrapper { Weapons = weapons2 };
                
                obj = jsonSerializer.Deserialize<Wrapper>(bsonReader);
                
                weapons2 = obj.Weapons;
                DsrlzList.ItemsSource = weapons2;
                

            }
            stream.Close();
            ActivateSaveCRCPlugins();
            ActivateCompareCRCPlugins();
            
        }

        private void AddToHollowsList_Click(object sender, RoutedEventArgs e)
        {
            Type weaponType = WeaponsTypes.SelectedItem.GetType();
            object newWeapons = Activator.CreateInstance(weaponType);            
            weapons.Add((Weapon)newWeapons);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if(WeaponsList.SelectedValue != null)
                ShowAllProperties(WeaponsList);
            else
            {
                ShowAllProperties(DsrlzList);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string path;
            OpenFileDialog fd = new OpenFileDialog();
            fd.ShowDialog();
            path = fd.FileName;
            Assembly asm = Assembly.LoadFrom(path);            
            hashTypes = asm.GetTypes();
            foreach (var type in hashTypes)
            {
                var plugin = asm.CreateInstance(type.FullName) as IHash;
                Adapter adr = new Adapter(plugin);
                hashCreatorsPlugins.Add(adr);                
                listOfExtensions.Items.Add(plugin);
                
            }

            
        }
    }
}
