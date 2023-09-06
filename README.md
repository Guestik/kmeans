# kmeans
K-means clustering in C#

Process details:

1. Since C# as an OOP language is used, two classes were created for input data (called DataElement) as well as for clusters (called Cluster)
2. Import dataset and instantiate DataElements
3. Set number of clusters (8 in this case)
4. Initialize clusters and assign a random data’s values to them (to its coordinates)
5. Find the nearest cluster to each data element and assign it
  a. Count Euclidean distance for each element between it and each cluster’s center point
  b. Assign the nearest cluster to the data element
  c. Center each cluster to mean of coordinates of its data element’s values
  d. Check convergence (if absolute value of every coordinate’s difference between newly calculated mean and old mean is less than 1, then I consider the coordinate convergent)
  e. Calculate reconstruction error
6. Export data with its clusters
