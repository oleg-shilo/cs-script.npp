using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CSScriptNpp
{
    class ToolStripPersistance
    {
        ToolStrip control;
        ToolStripItem[] originalButtonsOrder;
        string[] defaultLayout;
        FileSystemWatcher watcher;
        string file;

        public ToolStripPersistance(ToolStrip control, string settingsFile)
        {
            //Debug.Assert(false);
            this.control = control;
            this.file = settingsFile;
            this.originalButtonsOrder = control.Items.Cast<ToolStripItem>().ToArray();

            defaultLayout = new[]{"#Move lines up/down to change the button position in toolbar.",
                                  "#Replace '+' prefix with '-' to hide button."}
                                  .Concat(originalButtonsOrder.Select(x =>
                                                                      {
                                                                          if (x is ToolStripSeparator)
                                                                              return "---";
                                                                          else
                                                                              return "+" + x.Name;
                                                                      }))
                                  .ToArray();
        }

        public void Load()
        {
            //NOTE: while it is tempting to encode ToolStripItem visibility in the config file it cannot be done as
            //the items change their visibility because of the toolbar resizing.
            //Thus we need to encode ToolStripItem presence in toolbar items instead.

            if (watcher == null)
            {
                watcher = new FileSystemWatcher(Path.GetDirectoryName(file), Path.GetFileName(file));
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Changed += watcher_Changed;
            }

            watcher.EnableRaisingEvents = false;
            if (!File.Exists(file))
            {
                File.WriteAllLines(file, defaultLayout);
            }
            else
            {
                var layout = File.ReadAllLines(file)
                                 .Where(x => !string.IsNullOrWhiteSpace(x));

                if (!layout.Any())
                {
                    layout = defaultLayout;
                    File.WriteAllLines(file, layout);
                }

                ProcessLayout(layout);

                //<-|+><button name>
                var configuredButtonsNames = layout.Where(x => x != "---").Select(x => x.Substring(1));
                var nonConfiguredButtons = originalButtonsOrder.Where(x => !(x is ToolStripSeparator) && !configuredButtonsNames.Contains(x.Name));

                if (nonConfiguredButtons.Any())
                {
                    Dictionary<int, string> layoutChanges = UpdateLayout(nonConfiguredButtons);

                    var newLayout = layout.ToList();

                    foreach (int key in layoutChanges.Keys.OrderByDescending(x => x))
                        newLayout.Insert(key, layoutChanges[key]);

                    layout = newLayout.ToArray();
                    File.WriteAllLines(file, layout);
                }
            }
            watcher.EnableRaisingEvents = true;
        }

        DateTime lastLoadingTimestamp = DateTime.MinValue;

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            //do some extra checking as this event can be fired multiple times (e.g. N++ file save does it)
            if (lastLoadingTimestamp != File.GetLastWriteTime(file))
            {
                lastLoadingTimestamp = File.GetLastWriteTime(file);
                control.Invoke((Action)Load);
            }
        }

        Dictionary<int, string> UpdateLayout(IEnumerable<ToolStripItem> nonConfiguredButtons)
        {
            var result = new Dictionary<int, string>();
            foreach (ToolStripItem item in nonConfiguredButtons.Reverse())
            {
                int originalIndex = Array.IndexOf(originalButtonsOrder, item);
                if (control.Items.Count >= originalIndex)
                {
                    control.Items.Insert(originalIndex, item);
                    result.Add(originalIndex, "+" + item.Name);
                }
                else
                {
                    control.Items.Add(item);
                    result.Add(control.Items.Count, "+" + item.Name);
                }
            }
            return result;
        }

        void ProcessLayout(IEnumerable<string> layout)
        {
            control.Items.Clear();

            foreach (string name in layout)
            {
                if (name == "---")
                {
                    control.Items.Add(new ToolStripSeparator());
                }
                else if (name.StartsWith("+"))
                {
                    string itemName = name.Substring(1);
                    var item = originalButtonsOrder.Where(x => x.Name == itemName).FirstOrDefault();
                    if (item != null)
                    {
                        control.Items.Add(item);
                    }
                }
            }
        }

        //string[] Serialize(IEnumerable<ToolStripItem> items)
        //{
        //    string[] result = items.Select(x =>
        //                                    {
        //                                        string visibility = x.Visible ? "+" : "-";
        //                                        return x.Name.StartsWith("toolStripSeparator") ? "---" : visibility + x.Name;
        //                                    }).ToArray();

        //    return result;
        //}
    }
}