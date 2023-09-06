using System;
using System.Collections.Generic;
using System.IO;
using Accord.Math;
using System.Text.RegularExpressions;

namespace kMeans
{
    class Program
    {
        class Cluster
        {
            public int ID { get; private set; }
            public double[] Values { get; set; }
            public List<DataElement> DataElements { get; private set; }
            public double[] LastMeans { get; private set; }
            public bool IsConvergent { get; private set; }

            public Cluster(int id, double[] values) //Constructor
            {
                this.ID = id;
                this.Values = values;

                LastMeans = new double[Values.Length];
                DataElements = new List<DataElement>();
                IsConvergent = false;
            }

            public void AddElement(DataElement d) //Assign new Element to the Cluster
            {
                DataElements.Add(d);
            }

            public void ClearElements() //Clear assigned elements
            {
                DataElements.Clear();
            }

            public void CenterToMean() //Center cluster to the mean of its assigned elements
            {
                double[] means = new double[Values.Length];
                for (int i = 0; i < means.Length; i++)
                    means[i] = 0;

                //double meanX = 0;
                //double meanY = 0;
                for (int i = 0; i < DataElements.Count; i++)
                {
                    for (int y = 0; y < Values.Length; y++)
                    {
                        means[y] += DataElements[i].Values[y];
                    }
                }

                for (int i = 0; i < means.Length; i++)
                    means[i] = means[i] / DataElements.Count;

                Values = means;

                CheckConvergence(Values);

                LastMeans = means;
            }

            public void CheckConvergence(double[] newMeans)
            {
                bool tempCovergence = true;
                for (int i = 0; i < Values.Length; i++)
                {                    
                    if (Math.Abs(LastMeans[i] - newMeans[i]) >= 1)
                        tempCovergence = false;
                    //Console.WriteLine("{3}: {0} - {1}, {2}", LastMeans[i], newMeans[i], tempCovergence, ID);
                    //Console.ReadKey();
                }
                IsConvergent = tempCovergence;
            }
        }

        class DataElement
        {
            public double[] Values { get; private set; }
            public Cluster Class { get; private set; }
            public double DistanceToMyCluster { get; set; }

            public DataElement(double[] values) //Constructor
            {
                this.Values = values;
            }

            public void AsignCluster(Cluster c)
            {
                Class = c;
                c.AddElement(this);
            }
        }

        static void Main(string[] args)
        {
            //Set number of clusters
            int numOfClusters = 0;
            int maxNumOfClusters = 10;

            //Set number of dimensions of the data
            int dimensions = 7;

            //Import the dataset
            List<DataElement> inputData = new List<DataElement>();
            using (StreamReader reader = new StreamReader("customer_data.csv"))
            {
                //Regex dotPattern = new Regex("[.]"); //Need to replace ',' because otherwise can not parse numbers
                string line;
                //Read line by line
                while ((line = reader.ReadLine()) != null)
                {
                    string[] split = line.Split(',');

                    //for (int tempik = 0; tempik < dimensions; tempik++)
                    //{
                    //    split[tempik] = dotPattern.Replace(split[tempik].Trim(), ",");
                    //}

                    double[] vector = new double[dimensions];
                    for (int i = 0; i < dimensions; i++)
                    {
                        //Console.WriteLine(split[i]);
                        vector[i] = Math.Round(double.Parse(split[i]), 14);
                        //Console.WriteLine(vector[i]);
                        //Console.ReadKey();
                    }
                    inputData.Add(new DataElement(vector));
                }
            }
            Console.WriteLine("{0} data imported", inputData.Count);
            
            if (numOfClusters == 0)
            {
                Console.WriteLine("Going to run k-means for 1 - " + maxNumOfClusters + " clusters in order to find elbow point. To Start k-means, press any key");
                Console.ReadKey();
                Console.WriteLine("Starting K-means");
                //Start the K-means process (if we don't know the num of clusters
                for (int k = 1; k <= maxNumOfClusters; k++)
                {
                    double recError = Kmeans(k, ref inputData);
                    Console.WriteLine("Clusters: " + k + " Error: " + recError);
                }
            }
            else
            {
                Console.WriteLine("Going to run k-means for " + numOfClusters + " clusters. To Start k-means, press any key");
                Console.ReadKey();
                Console.WriteLine("Starting K-means");
                double recError = Kmeans(numOfClusters, ref inputData);
                Console.WriteLine("Error: " + recError);
            }

            //Export the dataset with corresponding clusters
            FileStream stream = new FileStream("result.txt", FileMode.Create);
            StreamWriter file = new StreamWriter(stream);
            using (file)
            {
                file.Write("x;y;c\n");
                for (int i = 0; i < inputData.Count; i++)
                {
                    for (int y = 0; y < inputData[i].Values.Length; y++)
                    {
                        //Console.Write("hodnota: " + inputData[i].Values[y] + " ");
                        file.Write("{0};", inputData[i].Values[y]);
                    }
                    //Console.Write("classka: " + inputData[i].Class.ID + "\n");
                    file.Write("{0}\n", inputData[i].Class.ID);
                }
            }

            Console.WriteLine("\n\nAll done");
            Console.ReadKey();
        }

        static double Kmeans(int numOfClusters, ref List<DataElement> inputData)
        {
            //Initialize list of clusters
            List<Cluster> clusters = new List<Cluster>();

            //Create clusters and assign a random data values to them
            Random nahoda = new Random();
            for (int i = 0; i < numOfClusters; i++)
            {
                int indexOfRandomDataElement = nahoda.Next(0, inputData.Count);
                clusters.Add(new Cluster(i, inputData[indexOfRandomDataElement].Values));
            }

            bool AllClustersConvergent = false;
            while (!AllClustersConvergent)
            {
                Console.Write(".");
                //Find the nearest cluster to each data element and assign it
                for (int i = 0; i < inputData.Count; i++)
                {
                    //Count Euclidean distance for each element between it and each cluster center point
                    double minDistance = 0;
                    Cluster theNearestCluster = null;
                    for (int c = 0; c < clusters.Count; c++)
                    {
                        double distance = Distance.Euclidean(inputData[i].Values, clusters[c].Values);
                        if (c == 0 || distance < minDistance)
                        {
                            minDistance = distance;
                            theNearestCluster = clusters[c];
                        }
                    }
                    //Assign the nearest cluster to the data element
                    inputData[i].DistanceToMyCluster = minDistance;
                    inputData[i].AsignCluster(theNearestCluster);
                }
                //Check convergence
                AllClustersConvergent = true;
                for (int c = 0; c < clusters.Count; c++)
                {
                    //Console.WriteLine("každý cluster center to mean: " + c);
                    clusters[c].CenterToMean();
                    if (!clusters[c].IsConvergent)
                        AllClustersConvergent = false;
                }
            }

            //Count Reconstruction error
            double reconError = new double();
            for (int i = 0; i < inputData.Count; i++)
            {
                reconError += inputData[i].DistanceToMyCluster;
            }

            Console.WriteLine("\n");

            for (int i = 0; i < clusters.Count; i++)
            {
                Console.Write("\tCluster " + clusters[i].ID + ":  ");
                for (int y = 0; y < clusters[i].Values.Length; y++)
                {
                    Console.Write(clusters[i].Values[y] + " ");
                }
                Console.WriteLine();
            }

            return reconError;
        }
    }
}
