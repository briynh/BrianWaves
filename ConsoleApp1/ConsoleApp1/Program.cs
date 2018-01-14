using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    class Program
    {
        static public void Banana()
        {

            List<List<double>> eegData = new List<List<double>>();
            List<List<double>> databaseData = new List<List<double>>();


            callBatch();
            
            bool exists = false;
            while(exists == false)
            {
                if (File.Exists(@"C:\\Users\\kenke\\museFiles\\masterBatchCallTest.csv"))
                {
                    exists = true;
                    Console.WriteLine("File has been found");
                    System.Threading.Tasks.Task.Delay(25000).Wait();
                }
            }
            //Console.WriteLine("System is sleeping for 35 seconds");
            //System.Threading.Tasks.Task.Delay(35000).Wait();
            Console.WriteLine("System has woken up");
            //List<List<double>> eegList = new List<List<double>>();
            List<List<string>> blinkList = new List<List<string>>();
            List<List<string>> jawList = new List<List<string>>();
            int blink_Count = 0;
            int jaw_Count = 0;

            using (var reader = new StreamReader(@"C:\\Users\\kenke\\museFiles\\masterBatchCallTest.csv"))
            {
                
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var tokens = line.Split(',');
                    string item = tokens[1].ToString();
                    
                    if (item.ToString() == " /muse/elements/blink")
                    {
                        string time = tokens[0].ToString();
                        string item2 = tokens[2].ToString();
                        if (item2 == " 1")
                        {
                            blink_Count++;
                            //Console.WriteLine("User blinked");
                        }
                        blinkList.Add(new List<string> { time, item, item2 });
                    }

                    if (item.ToString() == " /muse/elements/jaw_clench")
                    {
                        string time = tokens[0].ToString();
                        string item2 = tokens[2].ToString();
                        if (item2 == " 1")
                        {
                            if (blinkList[blinkList.Count-1][2]== " 0")
                            {
                                jaw_Count++;
                                //Console.WriteLine("User clenched jaw");
                            }
                            
                        }

                        blinkList.Add(new List<string> { time, item, item2 });
                    }

                    if (item.ToString() == " /muse/eeg")
                    {
                        double time = Convert.ToDouble(tokens[0]);

                        double item2 = 0;
                        double item3 = 0;
                        double item4 = 0;
                        double item5 = 0;

                        if (tokens[2] != null) {
                            item2 = Convert.ToDouble(tokens[2]);
                        }
                        if (tokens[3] != null)
                        {
                            item3 = Convert.ToDouble(tokens[3]);
                        }
                        if (tokens[4] != null)
                        {
                            item4 = Convert.ToDouble(tokens[4]);
                        }
                        if (tokens[5] != null)
                        {
                            item5= Convert.ToDouble(tokens[5]);
                        }
                        eegData.Add(new List<double> { time, item2, item3, item4, item5 });
                    }
                }
            }
            using (var reader = new StreamReader(@"C:\\Users\\kenke\\museFiles\\BrianCSV.csv"))
            {

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var tokens = line.Split(',');
                    string item = tokens[1].ToString();

                    if (item.ToString() == " /muse/elements/blink")
                    {
                        string time = tokens[0].ToString();
                        string item2 = tokens[2].ToString();
                        if (item2 == " 1")
                        {
                            blink_Count++;
                            //Console.WriteLine("User blinked");
                        }
                        blinkList.Add(new List<string> { time, item, item2 });
                    }

                    if (item.ToString() == " /muse/elements/jaw_clench")
                    {
                        string time = tokens[0].ToString();
                        string item2 = tokens[2].ToString();
                        if (item2 == " 1")
                        {
                            if (blinkList[blinkList.Count - 1][2] == " 0")
                            {
                                jaw_Count++;
                                //Console.WriteLine("User clenched jaw");
                            }

                        }

                        blinkList.Add(new List<string> { time, item, item2 });
                    }

                    if (item.ToString() == " /muse/eeg")
                    {
                        double time = Convert.ToDouble(tokens[0]);

                        double item2 = 0;
                        double item3 = 0;
                        double item4 = 0;
                        double item5 = 0;

                        if (tokens[2] != null)
                        {
                            item2 = Convert.ToDouble(tokens[2]);
                        }
                        if (tokens[3] != null)
                        {
                            item3 = Convert.ToDouble(tokens[3]);
                        }
                        if (tokens[4] != null)
                        {
                            item4 = Convert.ToDouble(tokens[4]);
                        }
                        if (tokens[5] != null)
                        {
                            item5 = Convert.ToDouble(tokens[5]);
                        }
                        databaseData.Add(new List<double> { time, item2, item3, item4, item5 });

                    }


                }

            }
            eegData = normalizeAndInterpolateTimes(eegData);
            eegData = averageOutValues(eegData);
            databaseData = normalizeAndInterpolateTimes(databaseData);
            databaseData = averageOutValues(databaseData);

            int retval = CompareValues(eegData, databaseData);
            if (retval == 0)
            {
                Console.WriteLine("You are ME!");
            }
            if (retval == 100)
            {
                Console.WriteLine("You are NOT ME!");
            }
            Console.ReadKey();

        }

        static void callBatch()
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "C:\\Users\\kenke\\museFiles\\batchscripts\\masterBatchCall.bat";
            proc.StartInfo.WorkingDirectory = "C:\\Users\\kenke\\museFiles\\batchscripts\\";
            proc.Start();
        }











        static int CompareValues(List<List<double>> receivedData, List<List<double>> databaseData)
        {
            /* This function assumes that receivedData and databaseData have already been run through normalizeAndInterpolateTimes()
                as well as averageOutValues() */
            /* We first attempt to estimate the offset of the data, by differencing the average values of receivedData and databaseData
                (averaged over number of samples, not time). 
                We then allow each data point to have some leeway
                and finally we account for outliers by having a threshold of failures, dependant again on the number of samples. */



            double[] difference = { 0, 0, 0, 0 };

            int receivedLength = receivedData.Count;

            int databaseLength = databaseData.Count;

            double asdf = (receivedLength > databaseLength ? databaseLength / 10000 : receivedLength / 10000);

            double[] failureThreshold = { asdf, asdf, asdf, asdf };


            //OFFSET VALUES:
            double[] receivedAverage = { 0, 0, 0, 0 };
            double[] databaseAverage = { 0, 0, 0, 0 };
            for (int i = 0; i < receivedLength; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    
                    //Console.WriteLine(j);
                    receivedAverage[j] += receivedData[i][j+1];
                    if(j == 1)
                    {
                        //Console.WriteLine(receivedAverage[j]);
                    }
                }
            }
            for (int i = 0; i < databaseLength; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    databaseAverage[j] += databaseData[i][j+1];
                }
            }
            for (int j = 0; j < 4; ++j)
            {
                receivedAverage[j] /= receivedLength;
                databaseAverage[j] /= databaseLength;
                difference[j] = databaseAverage[j] - receivedAverage[j];
            }
            Console.WriteLine("Received Average:");
            Console.WriteLine("{0},{1},{2},{3}",receivedAverage[0], receivedAverage[1], receivedAverage[2], receivedAverage[3]);
            Console.WriteLine("Database Average:");
            Console.WriteLine("{0},{1},{2},{3}", databaseAverage[0], databaseAverage[1], databaseAverage[2], databaseAverage[3]);
            //LEEWAY:
            int j_index = 0;
            double timingleeway = 0.003;
            double[] leeway = { 50, 50, 50, 50 }; //MESS WITH THESE VALUES
            int wrongCounter = 0;
            double checker;
            for (int i = 0; i < 4; i++)
            {
                checker = receivedAverage[i] - databaseAverage[i];
                if (checker < 0)
                {
                    checker = -1 * checker;
                }
                if (checker > leeway[i])
                {
                    wrongCounter++;
                }
            }
            if (wrongCounter > 1)
            {
                return 100;
            }



            for (int i = 0; i < receivedLength; ++i)
            {
                for (int j = j_index; j < j_index + 100; ++j)
                {
                    if (j >= databaseLength)
                    {
                        break;
                    }
                    if (isWithin(databaseData[j][0], receivedData[i][0], timingleeway))
                    {
                        j_index = j;
                        break;
                    }
                }
                for (int k = 0; k < 4; ++k)
                {
                    if (!isWithin(databaseData[j_index][k], receivedData[i][k], leeway[k]))
                    {       //Iffy on the logic of this line
                        --failureThreshold[k];
                        if (failureThreshold[k] == 0)
                        {
                            return 100; //FAILED!
                        }
                    }
                }
            }
            return 0;
        }

        static bool isWithin(double one, double two, double threshold, bool signed = false)
        {
            double diff = one - two;
            if (signed == false)
            {
                diff = (diff < 0 ? -1 * diff : diff);
            }
            if (diff < threshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        static List<List<double>> averageOutValues(List<List<double>> data)
        {
            /* adds up the 50 values surrounding each data point, averages the value,
		        and stores it back in the data point. Gets rid of rapid jumps */

            int interval = 51; //The number of indices over which to average out values (keep as an ODD value)
            int datalength = data.Count;

            for (int i = 0; i < datalength; ++i)
            {
                double sumValueOne = 0;
                double sumValueTwo = 0;
                double sumValueThree = 0;
                double sumValueFour = 0;

                for (int j = i - interval / 2; j <= i + interval / 2; ++j)
                {
                    if (j < 0 || j >= datalength)
                    {
                        continue;
                    }
                    sumValueOne += data[j][1];
                    sumValueTwo += data[j][2];
                    sumValueThree += data[j][3];
                    sumValueFour += data[j][4];
                }

                data[i][1] = sumValueOne / 51.0;
                data[i][2] = sumValueTwo / 51.0;
                data[i][3] = sumValueThree / 51.0;
                data[i][4] = sumValueFour / 51.0;
            }

            return data;
        }



        static List<List<double>> normalizeAndInterpolateTimes(List<List<double>> data)
        {
            double normalizeTimes = data[0][0];
            double currTime = 0;
            double nextTime = 0;

            //Subtract the first timestamp so the times are always in a similar range:
            for (int i = 0; i < data.Count; ++i) { data[i][0] -= normalizeTimes; }

            double previousFloorTime = data[0][0];
            int previousFloorIndex = 0;

            double nextFloorTime = 0;
            int nextFloorIndex = 0;

            for (int i = 0; i < data.Count - 1; ++i)
            {
                nextTime = data[i + 1][0];
                currTime = data[i][0];

                if (nextTime > currTime)
                {
                    nextFloorIndex = i + 1;
                    nextFloorTime = data[i + 1][0];
                    for (int j = previousFloorIndex + 1; j < nextFloorIndex; ++j)
                    {
                        //To the time, add the value of: slope between the 'corners' of the times * number of data points between this data point and the previous 'corner' time's data point.
                        data[j][0] += (nextFloorTime - previousFloorTime) / (nextFloorIndex - previousFloorIndex) * (j - previousFloorIndex);
                    }

                    previousFloorIndex = nextFloorIndex;
                    previousFloorTime = nextFloorTime;
                }
            }
            return data;
        }







    }
}
