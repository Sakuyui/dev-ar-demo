using System.Collections.Generic;
using System;
using UnityEngine;
using System.Text;
using System.Linq;

namespace AR.ActivationControl
{
    public record IndirectedEdge
    {
        public int vertex1 { get; set; }
        public int vertex2 { get; set; }
    }

    


    public class ConnectionGraph : IComparable<ConnectionGraph>, IComparable<ConnectionGraphCluster>
    {
        public int activateObjectID;
        public List<IndirectedEdge> edges = new List<IndirectedEdge>();

        public int CompareTo(ConnectionGraph? other)
        {
            if (other == null) return -2;
            HashSet<IndirectedEdge> edgesOfGraph1 = this.edges.ToHashSet();
            HashSet<IndirectedEdge> edgesOfGraph2 = other.edges.ToHashSet();
            bool flag1 = edgesOfGraph1.IsSubsetOf(edgesOfGraph2);
            bool flag2 = edgesOfGraph2.IsSubsetOf(edgesOfGraph1);
            if (flag1 && flag2)
            {
                return 0;
            }
            if (flag1) { return -1; }
            if (flag2) { return 1; }
            return -2;
        }

        public int CompareTo(ConnectionGraphCluster? other)
        {
            if (other == null) return -2;
            return this.CompareTo(other.GetItem(0));
        }

    }

    public class ConnectionGraphCluster : IComparable<ConnectionGraphCluster>
    {
        public List<ConnectionGraph> graphs = new List<ConnectionGraph>();

        public static List<ConnectionGraphCluster> ClusteringConnectionGraph(List<ConnectionGraph> graphs)
        {

            List<ConnectionGraphCluster> clusters = new List<ConnectionGraphCluster>();
            foreach (var graph in graphs)
            {
                ConnectionGraphCluster graphCluster = new ConnectionGraphCluster()
                {
                    graphs = new List<ConnectionGraph> { graph }
                };
                clusters.Add(graphCluster);
            }
            UnityEngine.Debug.Log("Init Clusters Finished...");

            bool dirty = false;
            if (clusters.Count <= 1) return clusters;
            int length = clusters.Count;
            do
            {
                dirty = false;
                for (int i = 0; i < length; i++)
                {
                    for (int j = i + 1; j < length; j++)
                    {
                        int compareResult = clusters[i].CompareTo(clusters[j]);
                        if (compareResult != -2)
                        {
                            UnityEngine.Debug.Log($"in i={i}, j={j}. Need Merging. compare result = {compareResult} ...");
                        }
                        if (compareResult == -1 || compareResult == 0) // clusters[i] is subgraph of cluster[j]
                        {
                            ConnectionGraphCluster c = MergeCluster(clusters[j], clusters[i]);
                            dirty = true;
                            var instance1 = clusters[i];
                            var instance2 = clusters[j];
                            clusters.Remove(instance1);
                            clusters.Remove(instance2);
                            clusters.Add(c);
                            length -= 1;
                        }
                        else if (compareResult == 1) // clusters[j] is subgraph of cluster[i]
                        {
                            ConnectionGraphCluster c = MergeCluster(clusters[i], clusters[j]);
                            dirty = true;

                            var instance1 = clusters[i];
                            var instance2 = clusters[j];
                            clusters.Remove(instance1);
                            clusters.Remove(instance2);
                            clusters.Add(c);
                        }
                        if (dirty) break;
                    }
                    if (!dirty) break;
                }
            } while (dirty && length > 1);

            return clusters;

        }




        public virtual int Count()
        {
            return this.graphs.Count;
        }
        public virtual ConnectionGraph GetItem(int index)
        {
            return graphs[index];
        }

        private static ConnectionGraphCluster MergeCluster(ConnectionGraphCluster connectionGraphCluster1, ConnectionGraphCluster connectionGraphCluster2)
        {
            ConnectionGraphCluster _MergeCluster(ConnectionGraphCluster cluster1, ConnectionGraphCluster cluster2)
            {
                var n1 = cluster1.Count();
                var n2 = cluster2.Count();
                if (n1 == 0) return connectionGraphCluster2;
                if (n2 == 0) return connectionGraphCluster1;
                ConnectionGraph[] graphs = new ConnectionGraph[n1 + n2];
                int i = 0, j = 0, k = 0;
                while (i < n1 && j < n2)
                {
                    var item1 = cluster1.GetItem(i);
                    var item2 = cluster2.GetItem(j);
                    if (item1.CompareTo(item2) >= 0)
                    {
                        graphs[k] = cluster1.GetItem(i);
                        i++;
                    }
                    else
                    {
                        graphs[k] = cluster2.GetItem(j);
                        j++;
                    }
                    k++;
                }
                while (i < n1)
                {
                    graphs[k] = cluster1.GetItem(i);
                    i++;
                    k++;
                }
                while (j < n2)
                {
                    graphs[k] = cluster2.GetItem(j);
                    j++;
                    k++;
                }
                return new ConnectionGraphCluster()
                {
                    graphs = graphs.ToList()
                };
            }

            var result = _MergeCluster(connectionGraphCluster1, connectionGraphCluster2);
            return result;
        }

        public virtual int CompareTo(ConnectionGraphCluster? other)
        {
            if (other == null || other.graphs == null || other.graphs.Count == 0 || this.graphs.Count == 0)
                return -2;
            return this.graphs.First().CompareTo(other.graphs.First());
        }
    }

}



