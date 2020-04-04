using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    class VariablesAjuster
    {
        Dictionary<int, HashSet<String>> optimalResults;
        public VariablesAjuster(String resultsPath)
        {
            optimalResults = new Dictionary<int, HashSet<String>>();
            String content = File.ReadAllText(resultsPath);
            String[] lines = content.Split('\n');
            int currentID = 351;
            optimalResults.Add(351, new HashSet<String>());
            int currentOrder = 1;
            for(int i = 0; i < lines.Length; i++)
            {
                String[] buffer = lines[i].Split(' ');
                if (Int32.Parse(buffer[0]) != currentID)
                {
                    currentID = Int32.Parse(buffer[0]);
                    optimalResults.Add(currentID, new HashSet<String>());
                    currentOrder = 1;
                }
                if (buffer[3].Equals("1\r"))
                {
                    optimalResults[currentID].Add(buffer[2]);
                    currentOrder++;
                }
            }
        }
        public int compareResults(Query query,Dictionary<string,double> qres)
        {
            List<KeyValuePair<string, double>> sorted = (from kv in qres orderby kv.Value select kv).ToList();
            sorted.Reverse();
            List<String> docRes = new List<string>();
            int result = 0;
            for(int i=0; i < sorted.Count; i++)
            {
                docRes.Add(sorted[i].Key);
            }
            Console.WriteLine("Query: " + query.getNum(true));
            HashSet<String> qOptimal = this.optimalResults[Int32.Parse(query.getNum(true))];
            for(int i = 0; i < 50; i++)
            {
                //Console.WriteLine(Int32.Parse(query.getNum()) + " 0 " + docRes[i] + " 1 0 r");
                if (qOptimal.Contains(docRes[i]))
                {
                    result++;
                    Console.Write(i + ",");
                }
            }
            Console.WriteLine(" ");
            return result;
        }
        /***
         * function for performing set manipulations such as adding of subtracting values of vec2 from all values that exist in vec1
         */
        public Dictionary<string,double> manipulateResults(Dictionary<string,double> vec1,Dictionary<string,double> vec2,string operation)
        {
            String[] vec1Keys = vec1.Keys.ToArray();
            for(int i = 0; i < vec1Keys.Length; i++)
            {
                if (vec2.Keys.Contains(vec1Keys[i]))
                {
                    switch (operation)
                    {
                        case "add":
                            vec1[vec1Keys[i]] += vec2[vec1Keys[i]];
                            break;
                        case "substract":
                            vec1[vec1Keys[i]] -= vec2[vec1Keys[i]];
                            break;
                        default:
                            continue;
                    }
                }
            }
            return vec1;
        }
        public static void Main()
        {
            Parser parser = new Parser(@"stop_words.txt", false);
            VariablesAjuster va = new VariablesAjuster(@"qrel.txt");
            //QueryMutator qm = new QueryMutator(@"X:\Junk\glove.6B.100dc.vec", 1);
            Ranker ranker = new Ranker(@"D:\Posting", false, @"C:\a\glove.6B.100dc.vec");
            double b = 0.00;
            double k = 1.2;
            double maxB = 0;
            double maxK = 0;
            int max = 0;
            String queries = File.ReadAllText(@"queries.txt");
            String[] q = queries.Split(new string[] { "\r\n\r\n\r\n" },StringSplitOptions.RemoveEmptyEntries);
            Token[][] arr = new Token[15][];
            Token[][] relevant = new Token[15][];
            //Token[][] irrelevant = new Token[15][];
            Query[] col = new Query[15];
            for(int i = 0; i < 15; i++)
            {
                col[i] = new Query(q[i]);
                arr[i] = parser.processDoc(new Document(null, null, null, null, col[i].getQuery(), null));
                relevant[i] = parser.processDoc(new Document(null, null, null, null, col[i].getRelevant(), null));
                //irrelevant[i] = parser.processDoc(new Document(null, null, null, null, col[i].getNonRelevant(), null));
            }

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    ranker.b = b;
                    ranker.k = k;
                    int score = 0;
                    //calculate query and compare
                    for (int ki = 0; ki < 15; ki++)
                    {
                        //Token[][] mutated = qm.getPermutations(arr[k]);
                        Dictionary<string,double> queryResult = ranker.processQuery(arr[ki],relevant[ki]);
                        //Dictionary<string, double> queryResult = ranker.processQuerySemantically(arr[k], relevant[k]);

                        //Dictionary<string, double> irrelevantResults = ranker.processQuery(irrelevant[k]);
                        //queryResult = va.manipulateResults(queryResult, irrelevantResults, "substract");
                        score += va.compareResults(col[ki], queryResult);
                    }
                    Console.WriteLine("k=" + Math.Round(k, 2) + " b=" + Math.Round(b, 2) + " Score: " + score);
                    //compare with max if larger - update
                    if (score > max)
                    {
                        max = score;
                        maxB = b;
                        maxK = k;
                    }
                    b += 0.05;
                }
                k += 0.05;
                b = 0.00;
            }
            Console.WriteLine("MAX: B=" + Math.Round(maxB, 2) + " K=" + Math.Round(maxK, 2) + " Score: " + max);
        }
    }
}
