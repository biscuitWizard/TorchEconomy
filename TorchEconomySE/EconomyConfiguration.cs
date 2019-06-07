using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;
using Torch.Views;

namespace TorchEconomySE
{
    public class EconomyConfiguration : ViewModel
    {
        public EconomyConfiguration()
        {

        }

        [Display(Name = "Pack Planets", GroupName = "Database Settings", Order = 0, Description = "")]
        private string _connectionString;
        public string ConnectionString { get => _connectionString; set => SetValue(ref _connectionString, value); }

        private bool _sqlLite;
        public bool SqlLite { get => _sqlLite; set => SetValue(ref _sqlLite, value); }

        private bool _mysql;
        public bool MySQL { get => _mysql; set => SetValue(ref _mysql, value); }

    }
}
