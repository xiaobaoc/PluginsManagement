using PluginsBase;
using System.IO;

namespace PluginsManagement
{
    public partial class Form1 : Form
    {
        private readonly PluginManager _pluginManager;
        public Form1()
        {
            InitializeComponent();

            _pluginManager = new PluginManager(Path.Combine(Application.StartupPath, "Plugins"));

            _pluginManager.PluginsUpdated += UpdatePluginList;
            CmdClick += _pluginManager.ExecuteFunc;
        }

        private void UpdatePluginList()
        {
            try
            {
                if(this.InvokeRequired)
                {
                    this.Invoke(UpdatePluginList);
                    return;
                }
                pluginsMenu.DropDownItems.Clear();
                Dictionary<string, Guid> menuGuids = new Dictionary<string, Guid>();

                foreach (var plugin in _pluginManager.Plugins)
                {
                    if (!string.IsNullOrEmpty(plugin.Menu) && !string.IsNullOrEmpty(plugin.Name) && plugin.Guid != default(Guid))
                    {
                        var menuItemName = plugin.Menu;
                        if (menuGuids.ContainsKey(menuItemName) && menuGuids[menuItemName] == plugin.Guid)
                        {
                            var existingMenuItem = pluginsMenu.DropDownItems.OfType<ToolStripMenuItem>()
                               .FirstOrDefault(menuItem => menuItem.Text.Equals(menuItemName));
                            if (existingMenuItem != null)
                            {
                                existingMenuItem.DropDownItems.Add(CreatePluginMenuItem(plugin));
                            }

                        }

                        else
                        {
                            var newFirstLevelMenuItem = new ToolStripMenuItem(plugin.Menu);
                            var newSecondLevelMenuItem = CreatePluginMenuItem(plugin);
                            newFirstLevelMenuItem.DropDownItems.Add(newSecondLevelMenuItem);
                            pluginsMenu.DropDownItems.Add(newFirstLevelMenuItem);

                            menuGuids[menuItemName] = plugin.Guid;
                        }
                    }

                    
                }

            }
            catch (InvalidOperationException)
            {
                UpdatePluginList();
                return;
            }
            catch
            {

            }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _pluginManager.LoadPlugins();
            UpdatePluginList();
        }
        protected override void OnClosed(EventArgs e)
        {
            _pluginManager.UnloadPlugins();
            base.OnClosed(e);

        }


        private void Form1_Load(object sender, EventArgs e)                                             
        {
           
        }

        event Action<(Guid, string, string)> CmdClick;
        private ToolStripMenuItem CreatePluginMenuItem(IPlugin plugin)
        {
            var menuItem =new ToolStripMenuItem(plugin.Name);
            menuItem.Click += (sender, e) =>
            {
                CmdClick?.Invoke((plugin.Guid, plugin.Menu, plugin.Name));
            };

            return menuItem;
        }
    }

   
}
                                                                                                    
