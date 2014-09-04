using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snowplow.Tracker
{
    public struct TransactionItem
    {
        public string sku;
        public double price;
        public int quantity;
        public string name;
        public string category;

        public TransactionItem(string sku, double price, int quantity, string name = null, string category = null)
        {
            this.sku = sku;
            this.price = price;
            this.quantity = quantity;
            this.name = name;
            this.category = category;
        }
    }
}
