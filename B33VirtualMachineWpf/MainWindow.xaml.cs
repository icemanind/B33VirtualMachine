using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace B33VirtualMachineWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly B33Cpu.B33Cpu _cpu;

        public MainWindow()
        {
            InitializeComponent();

            _cpu = new B33Cpu.B33Cpu();
            _cpu.LoadProgram(@"C:\Users\Alan\Documents\TicTacToeBare.B33");
            _cpu.Hardware.Add(B33MainScreen);
            _cpu.Hardware.Add(new B33Cpu.Hardware.B33Keyboard(this));
            _cpu.Hardware.Add(new B33Cpu.Hardware.B33Sound());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ////B33MainScreen.Poke(40960, 65);
            ////B33MainScreen.Poke(40960 + 2, 66);
            ////B33MainScreen.Poke(40961, 31);
            ////B33MainScreen.Poke(40961+2, 31);
            ////B33MainScreen.Poke(40961+80, 31);
            //int num = 49;

            //for (int i = 0; i < 1000; i += 2)
            //{
            //    B33MainScreen.Poke((ushort) (40960+i), (byte) num);
            //    B33MainScreen.Poke((ushort) (40961 + i), (byte) 31);
            //    num++;
            //    if (num > 57)
            //        num = 48;

            //}

            //B33MainScreen.Poke(40961+80, 63);
            _cpu.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            B33MainScreen.ScrollUp();
        }
    }
}
