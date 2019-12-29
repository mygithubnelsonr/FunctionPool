using System.Collections.Generic;
using System.Xml.Linq;

namespace NRSoft.FunctionPool
{
    public static class CDAID
    {
        public static string XMLFile { get; set; }
        public static string Artist { get; set; }
        public static string CDTitel { get; set; }
        public static string Genre { get; set; }

        public static List<string> TrackList = new List<string>();

        public static void ReadDb()
        {
            TrackList.Clear();
            
            // XML File laden
            try
            {
                var cds = XElement.Load(XMLFile);

                // Ausgabe aller CD's in einer Schleife:
                foreach (var cd in cds.Elements())
                {
                    CDTitel = cd.Element("title").Value;
                    Genre = cd.Element("id3genre").Value;
                    Artist = cd.Element("artist").Value;

                    var tracks = new XElement(cd.Element("tracks"));

                    foreach (var track in tracks.Elements())
                    {
                        TrackList.Add(track.Element("tracknum").Value + "||" +
                            track.Element("ttitle").Value);
                    }
                }
            }
            catch
            {
                TrackList.Add("||could not read xml file");
            }
        }

    }

}
