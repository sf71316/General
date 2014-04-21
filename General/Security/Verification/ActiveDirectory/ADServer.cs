using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;

namespace General.Security
{
    public class ADServer
    {
        #region Variable
        private DirectoryEntry de;
        private DirectorySearcher ds;
        #endregion
        #region Property
        public string LoginName { get; set; }
        public string LoginPassword { get; set; }
        public string Domain { get; set; }
        public string ServerIP { get; set; }
        #endregion
        #region Constructor
        public ADServer(string name, string password, string domain,string serverip):base()
        {
            this.LoginName = name;
            this.LoginPassword = password;
            this.Domain = domain;
            this.ServerIP = serverip;
        }
        protected ADServer()
        {
            string QueryString = "LDAP://" + ServerIP + "/" + GetDomainName(this.Domain);
            de = new DirectoryEntry(QueryString, LoginName, LoginPassword);
        }
        #endregion
        public SearchResult SearchOne()
        {
            this.OnSearching();
            return ds.FindOne();
        }
        public SearchResultCollection SearchAll()
        {
            this.OnSearching();
            return ds.FindAll();
        }
        protected virtual void OnSearching()
        {
            ds = new DirectorySearcher(de);
            ds.PropertiesToLoad.Add("SAMAccountName");//account
            ds.PropertiesToLoad.Add("Name");//full name
            ds.PropertiesToLoad.Add("displayName");
            ds.PropertiesToLoad.Add("mail");
            ds.PropertiesToLoad.Add("description");
            ds.PropertiesToLoad.Add("phsicalDeliveryOfficeName");
            ds.PropertiesToLoad.Add("userPrincipalName");//user logon name,xxx@cccc.com
            ds.PropertiesToLoad.Add("telephoneNumber");
            ds.PropertiesToLoad.Add("givenName");//first name
            if (this.Searching != null)
            {
                this.Searching(this, new ADServerSearchingArgs(ds));
            }
        }
        private string GetDomainName(string domain)
        {
            string[] SplitStr = null;
            string DomainName = "";
            //Domain
            if (domain.Contains("."))
            {
                SplitStr = domain.Split('.');

                foreach (string item in SplitStr)
                {
                    if (DomainName == "")
                    {
                        DomainName += "DC=" + item;
                    }
                    else
                    {
                        DomainName += "," + "DC=" + item;
                    }
                }
            }
            else
            {
                DomainName = "DC=" + domain;
            }
            return DomainName;
        }
        #region Event
        public event EventHandler<ADServerSearchingArgs> Searching;
        #endregion
    }
}
