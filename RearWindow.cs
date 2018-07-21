using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;


//project currently runs in quadratic time due to making separate arrays for addresses and names
namespace RearWindow
{
    class RearWindow
    {
        //connects and runs separate member functions
        public static void Main(string[] args)  
        {
            string toFile = "";
            string user = Environment.UserName; //important in case client wants to save file
            bool save;
            
            //if they want to save, will run UpdateToFile() and update 'save' and 'ToFile'
            Console.WriteLine("Would you like to save neighbors to your Desktop? y/n: ");
            string temp = Console.ReadLine();
            if (temp.Equals("y"))
            {
                Console.WriteLine(@"Okay, saved to your Desktop named RearWindow.txt.");
                save = true;
            }
            else if (temp.Equals("n"))
                save = false;
            else //kept simple so only two inputs are 'y' or 'n'
            {
                Console.WriteLine("You didn't enter y/n, so I'm assuming that's a no...");
                save = false;
            }

            //web pages and information used after this point is public information and open to the public
            //all people not wanting their information shared are not included in these webpages
            WebClient web = new WebClient();
            String html = web.DownloadString("http://www.ohioresidentdatabase.com/address/street/45244/cincinnati/treeridge-dr");
            Print(Stalk(html));
            if (save) //ran only if client wants to save data
                toFile = UpdateTOFile(Stalk(html), toFile);

            html = web.DownloadString("http://www.ohioresidentdatabase.com/address/street/45244/cincinnati/treeridge-dr?page=2");
            Print(Stalk(html));
            if (save)
                toFile = UpdateTOFile(Stalk(html), toFile);

            html = web.DownloadString("http://www.ohioresidentdatabase.com/address/street/45244/cincinnati/treeridge-dr?page=3");
            Print(Stalk(html));
            if (save)
                toFile = UpdateTOFile(Stalk(html), toFile);
            Console.Read();

            if (save)
                System.IO.File.WriteAllText(@"C:\Users\" + user + @"\Desktop\RearWindow.txt", toFile);
        }

        //runs regular expression on webpages to find names and addresses
        //returns a List of Nodes to be used by Print() and UpdateToFile()
        public static List<Node> Stalk(String html)
        {
            //(.*?) finds <strong> tab and matches it and </strong> as well as information between tags
            //@ makes takes the string as a string literal thus ignoring any potential escape characters
            //(.*?) works similarly for m2 except to find <span> tags
            MatchCollection m1 = Regex.Matches(html, @"<strong>(.*?)</strong>", RegexOptions.Singleline);
            MatchCollection m2 = Regex.Matches(html, @"<span class=\""mt col-black\"">(.*?)</span>", RegexOptions.Singleline);

            //store Matches in arrays to make pairing information easier (name[0] pairs with adddresses[0])
            Match[] names = new Match[m1.Count];
            Match[] addresses = new Match[m2.Count];

            m1.CopyTo(names, 0);
            m2.CopyTo(addresses, 0);

            List<Node> neighborhood = new List<Node>();

            for (int i = 0; i < m1.Count; i++) //loop through each Match pair
            {
                int temp = i;

                //Node described below
                Node newHouse = new Node();

                //finds all addresses that are the same. This finds which names are associated with which address
                for (int x = i; x < m1.Count; x++)
                {
                    if (!addresses[x].ToString().Equals(addresses[i].ToString())) //go until you're at next address
                    {
                        string output1 = Regex.Replace(addresses[i].ToString(), "<span class=\"mt col-black\">", ""); //gets rid of HTML tags to leave address
                        output1 = Regex.Replace(output1, "</span>", "");
                        newHouse.address = output1;
                        break;
                    }
                    //have to make special exception for end of array because it was infinite looping because temp was not updated correctly
                    if (x == m1.Count - 1)
                    {
                        string output1 = Regex.Replace(addresses[i].ToString(), "<span class=\"mt col-black\">", "");
                        output1 = Regex.Replace(output1, "</span>", "");
                        newHouse.address = output1;
                        temp++;
                        break;
                    }
                    temp++;
                }

                //finds all names associated with the previously found address. Gets rid of HTML tags
                for (int q = i; q < temp; q++)
                {
                    string output2 = Regex.Replace(names[q].ToString(), "<strong>", "");
                    output2 = Regex.Replace(output2, "</strong>", "");
                    newHouse.household.Add(output2);
                }

                neighborhood.Add(newHouse);
                i = temp - 1;
            }

            return neighborhood;
        }

        //formatted to print address followed by each neighbor at that address
        public static void Print(List<Node> neighborhood)
        {
            foreach (Node house in neighborhood)
            {
                Console.WriteLine(house.address + ": ");
                foreach (string person in house.household)
                {
                    Console.WriteLine(person);
                }
                Console.WriteLine();
            }
        }

        //updates 'toFile' string with the names and addresses
        //used only if client wishes to save information
        public static string UpdateTOFile(List<Node> neighborhood, string x)
        {
            foreach (Node house in neighborhood)
            {
                x += house.address + ":" + Environment.NewLine;
                foreach (string person in house.household)
                {
                    x += person + Environment.NewLine;
                }

                x += Environment.NewLine;
            }
            return x;
        }

    }

    //Node is a pair of the address (string) and people at that address (List<string>)
    class Node
    {
        public string address;
        public List<string> household;

        public Node(string a, string n)
        {
            address = a;
            household.Add(n);
        }

        public Node()
        {
            address = "";
            household = new List<string>();
        }
    }
}
