using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;

namespace NoXP.Scrcpy.UI
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.InitializeScrcpy();
        }


        private void InitializeScrcpy()
        {
            CommandRegister.Init();
            ProcessFactory.SetBasePath(string.Empty);

            ADBDevice.GetAvailableDevices();


            Subject<ADBDevice> sub = new Subject<ADBDevice>();
            ItemsControl items = this.Get<ItemsControl>("Items");
            foreach (ADBDevice device in ADBDevice.AllDevicesCollection)
                sub.OnNext(device);

            items.Bind(ItemsControl.ItemsProperty, sub);

            //StackPanel stackPanel = this.Get<StackPanel>("StackPanel") as StackPanel;
            //Panel template = this.Get<StackPanel>("Template") as StackPanel;

            //foreach (ADBDevice device in ADBDevice.AllDevicesCollection)
            //{

            //    TextBlock tmpBlock = new TextBlock();
            //    //tmpBlock.Parent = this.StackPanel;
            //    tmpBlock.Text = device.Serial;
            //    stackPanel.Children.Add(tmpBlock);
            //}
        }

    }
}
