using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace PlexPosterExport
{
    class Program
    {
        public static string PlexURL;
        public static string PlexToken;
        public static List<MovieItem> movieItems = new List<MovieItem>();
        public static List<TVItem> tvItems = new List<TVItem>();
        public static string thisFilePath;
        public static string thisFileName;

        static void Main(string[] args)
        {
            using (var cc = new ConsoleCopy("[" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + "] PlexPosterExport.log"))
            {
                PrintStartupMessage();
                try
                {
                    if (args[0] == "--u")
                    {
                        if (args[2] == "--t")
                        {
                            PlexURL = args[1]; //get URL from args
                            PlexURL = PlexURL.Replace("https://", "http://"); //replace https with http
                            PlexToken = args[3];
                            Console.Write("     Drag the library .csv from ExportTools onto this window and then press [ENTER]\r\n\r\n     File: ");
                            string filePath = Console.ReadLine(); //read the file the user drags in: 
                            Console.WriteLine(" ");
                            ProcessCSV(filePath); //read the passed in CSV file and add items to movieItems/tvItems
                            WritePostersToFile(); //fetch posters and write to file as "poster.png", "{film}-poster.png", and "{film}.png" to cover all bases
                            PrintEndMessage(); //print an "Export complete" block"
                        }
                        else { Console.WriteLine("     Token not passed correctly. Expected `--t xxxxxxxxx` e.g. `--t YOURTOKENVALUEHERE`"); }
                    }
                    else { Console.WriteLine("     URL not passed correctly. Expected `--u http://...:32400` e.g. `--u https://192.168.1.40:32400`"); }
                }
                catch (Exception e)
                {
                    Console.Write("     Plex URL (http://ip:port): ");
                    PlexURL = Console.ReadLine();
                    Console.Write("     Plex Token: ");
                    PlexToken = Console.ReadLine();
                    PlexURL = PlexURL.Replace("https://", "http://"); //replace https with http
                    Console.Write("\r\n     Drag the library .csv from ExportTools onto this window and then press [ENTER]\r\n\r\n     File: ");
                    string filePath = Console.ReadLine(); //read the file the user drags in: 
                    Console.WriteLine(" ");
                    ProcessCSV(filePath); //read the passed in CSV file and add items to movieItems/tvItems
                    WritePostersToFile(); //fetch posters and write to file as "poster.png", "{film}-poster.png", and "{film}.png" to cover all bases
                    PrintEndMessage(); //print an "Export complete" block"
                }
            }
        }

        static void ProcessCSV(string file)
        {
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    string FourthValue = "";
                    string FifthValue = "";
                    string SixthValue = "";

                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    string FirstValue = values[0].Replace("\"", "");
                    string SecondValue = values[1].Replace("\"", "");
                    string ThirdValue = values[2].Replace("\"", "");
                    //try to parse 4th and 5th values, if not, it's movie.
                    //if 4th and 5th exist, it's tv
                    try
                    {
                        FourthValue = values[3].Replace("\"", "");
                        FifthValue = values[4].Replace("\"", "");
                        SixthValue = values[5].Replace("\"", "");
                    } catch { }
                    //if 4th value still empty, add to tv
                    if (FourthValue != "")
                    {
                        tvItems.Add(new TVItem(FirstValue, SecondValue, ThirdValue, FourthValue, FifthValue, SixthValue));
                    }
                    //otherwise add to movie
                    else
                    {
                        movieItems.Add(new MovieItem(FirstValue, SecondValue, ThirdValue));
                    }
                }
            }
        }
        static void FetchPoster(string posterURL, string targetPath, string targetName)
        {
            var posterURLFull = PlexURL + posterURL + "?X-Plex-Token=" + PlexToken;
            using (WebClient client = new WebClient()) {
                //download poster  from URL to path of media
                //if targetPath contains .fileext_[something] strip back to ext then strip back to last item in array
                Regex regex = new Regex("S[0-9][0-9]E[0-9][0-9]");
                if (regex.IsMatch(targetPath)) {
                    Console.WriteLine("     WARNING: Duplicate episode found, writing posters to first instance.");
                    var targetPathNew = targetPath.Split('\\');
                    List<string> targetPathRebuilt = new List<string>();
                    foreach (var item in targetPathNew)
                    {
                        //if regex matches SxxExx then break
                        if (regex.IsMatch(item))
                        {
                            break;
                        }
                        else
                        {
                            targetPathRebuilt.Add(item);
                        }
                    }
                    //build a new path if dupes are found
                    targetPath = String.Join('\\', targetPathRebuilt);
                }
                try { client.DownloadFile(new Uri(posterURLFull), targetPath + "\\" + targetName); } catch (Exception e) { Console.WriteLine("     ERROR: Could not download poster"); }
            }
        }
        static void BuildPathVariables(string path)
        {
            //build path variables, remove the "file" name from path
            string[] currentPath = path.Split("\\");
            thisFileName = currentPath.Last();
            currentPath = currentPath.SkipLast(1).ToArray();
            thisFilePath = String.Join("\\", currentPath);
        }

        static void WritePostersToFile()
        {
            //if tv is not empty, process csv as tv show
            if (tvItems.Count != 0)
            {
                for (int i = 5200; i < tvItems.Count; i++)
                {
                    Console.WriteLine("     Current Episode: [" + i + "] " + tvItems[i].Series + " | S" + tvItems[i].Season + "E" + tvItems[i].Episode + " | " + tvItems[i].Title);
                    BuildPathVariables(tvItems[i].Path);
                    string thisFileNameNoExt = thisFileName.Substring(0, thisFileName.Length - 4);
                    FetchPoster(tvItems[i].PosterURL, thisFilePath, thisFileNameNoExt + ".png");
                }
            }
            //if movies is not empty, process csv as movies
            if (movieItems.Count != 0)
            {
                for (int i = 1; i < movieItems.Count; i++)
                {
                    Console.WriteLine("     Current Movie: [" + i + "] " + movieItems[i].Title);
                    BuildPathVariables(movieItems[i].Path);
                    string thisFileNameNoExt = thisFileName.Substring(0, thisFileName.Length - 4);
                    FetchPoster(movieItems[i].PosterURL, thisFilePath, thisFileNameNoExt + ".png");
                    FetchPoster(movieItems[i].PosterURL, thisFilePath, thisFileNameNoExt + "-poster.png");
                    FetchPoster(movieItems[i].PosterURL, thisFilePath, "poster.png");
                }
            }
        }

        static void PrintStartupMessage()
        {
            Console.WriteLine("     ###################################################");
            Console.WriteLine("     #               Plex Poster Exporter              #");
            Console.WriteLine("     #     Requires:                                   #");
            Console.WriteLine("     #                                                 #");
            Console.WriteLine("     #      WebTools-NG -> Settings -> Delimiter ';'   #");
            Console.WriteLine("     #                                                 #");
            Console.WriteLine("     #       MOVIES:                                   #");
            Console.WriteLine("     #     * Use WebTools-NG to create an export of    #");
            Console.WriteLine("     #       your library with 'Title', 'Poster url',  #");
            Console.WriteLine("     #       and 'Part File Combined' only             #");
            Console.WriteLine("     #                                                 #");
            Console.WriteLine("     #       TV EPISODES:                              #");
            Console.WriteLine("     #     * Use WebTools-NG to create an export of    #");
            Console.WriteLine("     #       your library with 'Title', 'Season',      #");
            Console.WriteLine("     #       'Episode', 'Poster url', and              #");
            Console.WriteLine("     #       'Part File Combined' only                 #");
            Console.WriteLine("     #                                                 #");
            Console.WriteLine("     #     * Drag the CSV output onto this window      #");
            Console.WriteLine("     #                                                 #");
            Console.WriteLine("     #     * Wait for your posters to extract          #");
            Console.WriteLine("     #                                                 #");
            Console.WriteLine("     #   https://github.com/WebTools-NG/WebTools-NG    #");
            Console.WriteLine("     ###################################################");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
        }
        static void PrintEndMessage()
        {
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            Console.WriteLine("     ###################################################");
            Console.WriteLine("     #                 EXPORT COMPLETE                 #");
            Console.WriteLine("     ###################################################");
            Console.WriteLine(" ");
            Console.WriteLine(" ");
        }
    }
    public class MovieItem
    {
        public string Title { get; set; }
        public string PosterURL { get; set; }
        public string Path { get; set; }

        public MovieItem(string title, string posterURL, string path)
        {
            Title = title;
            PosterURL = posterURL;
            Path = path;
        }
    }
    public class TVItem
    {
        public string Series { get; set; }
        public string Title { get; set; }
        public string Season { get; set; }
        public string Episode { get; set; }
        public string PosterURL { get; set; }
        public string Path { get; set; }
        public TVItem(string series, string title, string season, string episode, string posterURL, string path)
        {
            Series = series;
            Title = title;
            Season = season;
            Episode = episode;
            PosterURL = posterURL;
            Path = path;
        }
    }
    class ConsoleCopy : IDisposable
    {

        FileStream fileStream;
        StreamWriter fileWriter;
        TextWriter doubleWriter;
        TextWriter oldOut;

        class DoubleWriter : TextWriter
        {

            TextWriter one;
            TextWriter two;

            public DoubleWriter(TextWriter one, TextWriter two)
            {
                this.one = one;
                this.two = two;
            }

            public override Encoding Encoding
            {
                get { return one.Encoding; }
            }

            public override void Flush()
            {
                one.Flush();
                two.Flush();
            }

            public override void Write(char value)
            {
                one.Write(value);
                two.Write(value);
            }

        }

        public ConsoleCopy(string path)
        {
            oldOut = Console.Out;

            try
            {
                fileStream = File.Create(path);

                fileWriter = new StreamWriter(fileStream);
                fileWriter.AutoFlush = true;

                doubleWriter = new DoubleWriter(fileWriter, oldOut);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open file for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(doubleWriter);
        }

        public void Dispose()
        {
            Console.SetOut(oldOut);
            if (fileWriter != null)
            {
                fileWriter.Flush();
                fileWriter.Close();
                fileWriter = null;
            }
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }
        }

    }
}
