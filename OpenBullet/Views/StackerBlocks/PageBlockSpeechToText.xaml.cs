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
using RuriLib;

namespace OpenBullet.Views.StackerBlocks
{
    /// <summary>
    /// Interaction logic for PageBlockSpeechToText.xaml
    /// </summary>
    public partial class PageBlockSpeechToText : Page
    {
        BlockSpeechToText vm;
        public PageBlockSpeechToText(BlockSpeechToText block)
        {
            InitializeComponent();
            vm = block;
            DataContext = vm;
        }
    }
}
