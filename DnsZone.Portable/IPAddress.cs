using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnsZone
{
	public class IPAddress
	{
		public string Address
		{
			get;
			set;
		}

		public string SubnetMask
		{
			get;
			set;
		}

		public override string ToString()
		{
			return Address;
		}
	}
}
