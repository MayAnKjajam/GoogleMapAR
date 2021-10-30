using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class TerrainNeighbors : MonoBehaviour
{
	public bool setMapDistance = true;
	private int terrainNo = 0;

	void Start ()
    {
		try
        {
            int gridSize = (int)Mathf.Sqrt(transform.childCount);

			foreach (Transform t in transform)
            {
				if(t.GetComponent<Terrain>() != null)
                {
					terrainNo++;

					if(setMapDistance)
                        t.GetComponent<Terrain>().basemapDistance = t.GetComponent<Terrain>().terrainData.size.x * gridSize * 4;
				}
			}
		}
		catch{}
		
		if(terrainNo >= 4)
			SetTerrainNeighbors();

		if(transform.gameObject.GetComponent<Terrain>() != null)
			transform.GetComponent<Terrain>().basemapDistance = transform.GetComponent<Terrain>().terrainData.size.x * 4;
	}
	
	private void SetTerrainNeighbors ()
	{
		List<Terrain> terrains = new List<Terrain>();
		int splitSize = (int)Mathf.Sqrt(terrainNo);

		foreach (Transform t in transform)
			if(t.GetComponent<Terrain>() != null)
				terrains.Add(t.GetComponent<Terrain>());

#if UNITY_2018_3_OR_NEWER
        for (int i = 0; i < Mathf.Pow(splitSize, 2); i++)
            terrains[i].allowAutoConnect = true;
#else
        int counter = 0;

		for(int y = 0; y < splitSize ; y++)
		{
			for(int x = 0; x < splitSize; x++)
			{
				int indexLft = counter - 1;
				int indexTop = counter - splitSize;
				int indexRgt = counter + 1;
				int indexBtm = counter + splitSize;
					
				if(y == 0)
				{
					if(x == 0)
						terrains[counter].SetNeighbors(null, null, terrains[indexRgt], terrains[indexBtm]);
					else if(x == splitSize - 1)
						terrains[counter].SetNeighbors(terrains[indexLft], null, null, terrains[indexBtm]);
					else
						terrains[counter].SetNeighbors(terrains[indexLft], null, terrains[indexRgt], terrains[indexBtm]);
				}
				else if(y == splitSize - 1)
				{
					if(x == 0)
						terrains[counter].SetNeighbors(null, terrains[indexTop], terrains[indexRgt], null);
					else if(x == splitSize - 1)
						terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], null, null);
					else
						terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], terrains[indexRgt], null);
				}
				else
				{
					if(x == 0)
						terrains[counter].SetNeighbors(null, terrains[indexTop], terrains[indexRgt], terrains[indexBtm]);
					else if(x == splitSize - 1)
						terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], null, terrains[indexBtm]);
					else
						terrains[counter].SetNeighbors(terrains[indexLft], terrains[indexTop], terrains[indexRgt], terrains[indexBtm]);
				}
				counter++;
			}
		}
			
		for(int i = 0; i < Mathf.Pow(splitSize, 2) ; i++)
			terrains[i].Flush();
#endif
    }
}

