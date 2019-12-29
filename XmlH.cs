using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;

namespace NRSoft.FunctionPool
{
    public class XmlH
    {
        #region fields
        private string _sFile = "";
        private string _sNode = "";
		private string _sValue = "";
        #endregion fields

        #region public properties
        public string SetFile
        {
            get {return _sFile;}
            set { _sFile = value; }
        }

        public string SetNode
		{
            get {return _sNode;}
			set { _sNode = value; }
		}

        public string GetNodeValue
		{
			get { 
                _sValue = _getNodeValue();
                return _sValue;
                }
		}

        public string SNode { get => _sNode; set => _sNode = value; }

        public string SFile { get => _sFile; set => _sFile = value; }

        #endregion methods

        #region methodes
        private string _getNodeValue ()
        {
            // XML File laden
            if (_sFile == String.Empty)
            {
                return String.Empty;
            }

            // root XElement zum Lesen erzeugen
            XElement root = XElement.Load(_sFile);
            
            // direkter zugriff auf knoten
            IEnumerable<XElement> list = root.XPathSelectElements(_sNode);
            // Console.WriteLine( list.Count() );

            string retval = "";
            foreach (XElement el in list)
            {
                retval = el.Value;
                // Console.WriteLine(el);
                break;
            }
            return retval;
        }
        #endregion
    }
}
