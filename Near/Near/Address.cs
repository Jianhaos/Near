using Microsoft.Phone.Maps.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Near
{
    public static class Address
    {
        public static StringBuilder ShowLocation(QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in e.Result)
            {
                if (item.Information.Address.HouseNumber != "")
                    sb.Append(item.Information.Address.HouseNumber + " ");
                if (item.Information.Address.Street != "")
                    sb.Append(item.Information.Address.Street + ", ");
                if (item.Information.Address.City != "")
                    sb.Append(item.Information.Address.City + ", ");
                if (item.Information.Address.State != "")
                    sb.Append(item.Information.Address.State + ", ");
                if (item.Information.Address.PostalCode != "")
                    sb.Append(item.Information.Address.PostalCode + ", ");
                if (item.Information.Address.Country != "")
                    sb.Append(item.Information.Address.Country);
            }
            return sb;
        }

        public static StringBuilder ShowLocation(QueryCompletedEventArgs<IList<MapLocation>> e, string condition)
        {
            StringBuilder sb = new StringBuilder();
            if (condition == "city")
            {
                foreach (var item in e.Result)
                {
                    if (item.Information.Address.City != "")
                        sb.Append(item.Information.Address.City);
                    else
                    {
                        if (item.Information.Address.State != "")
                            sb.Append(item.Information.Address.State);
                        else
                        {
                            if (item.Information.Address.Country != "")
                                sb.Append(item.Information.Address.Country);
                        }
                    }
                }
            }
            return sb;
        }
    }
}
