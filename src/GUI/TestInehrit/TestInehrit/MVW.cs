using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestInehrit
{
    public class MVW
    {
        public ObservableCollection<Layer> List = new ObservableCollection<Layer>() { new Layer(), new Layer() };
        public Layer Layer { get; set; } = new Dense();
    }
}
