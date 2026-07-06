using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum WorldSize 
{
    _20 = 21,
    _50 = 51,
    _100 = 101,
    _200 = 201
}

public class RandomMapGenerator : MonoBehaviour
{
    #region Serializables

    [SerializeField]
    private WorldSize _sizeInNodes;
    [SerializeField]
    private List<Building> _buildingsToPlace;
    [SerializeField]
    private MapNode _nodePrefab;

    #endregion

    #region Miembros privados

    private List<Building> _keyBuildings;
    private List<Building> _impotantBuildings;
    private List<Building> _normalBuildings;
    private List<City> _cities;
    private MapNode[,] _primaryMatrix;
    private MapNode[,] _secondaryMatrix; 

    private int _initCoord;
    
    #endregion

    #region Eventos de Unity

    private void Awake()
    {
        _initCoord = ((int)_sizeInNodes -1) / 2;
        _primaryMatrix = new MapNode[(int)_sizeInNodes, (int)_sizeInNodes];
    }

    #endregion

    #region Metodos Privados

    private void GenerateMap(List<MapNode> map, List<Building> buildings)
    {
        SortNodes();
        PlaceKeyBuildings();
        PlaceNormalBuildings();
    }

    /// <summary>
    /// Crea todos los nodos del mapa.
    /// </summary>
    private void GenerateNodes() 
    {
        _primaryMatrix[_initCoord, _initCoord] = Object.Instantiate(_nodePrefab, new Vector3Int(0, 0, 0), Quaternion.identity);
    }

    /// <summary>
    /// Ordeno los nodos dentro de las listas que corresponda cada uno.
    /// </summary>
    private void SortNodes()
    {
        foreach (var item in _buildingsToPlace)
        {
            if (item.Settings.IsKeyBuilding) _keyBuildings.Add(item);
            else if (item.Settings.IsImportantBuilding) _impotantBuildings.Add(item);
            else _normalBuildings.Add(item);
        }
    }

    private void PlaceKeyBuildings()
    {
        while (_keyBuildings.Count > 0)
        {
            var building = _keyBuildings[_keyBuildings.Count];
            //Si el edificio puede repetirse en el mundo lo ubica la cantidad de veces que se configuro.
            for (int i = 0; i < building.Settings.AmountPerWorld; i++)
                PlaceBuildingInWorld(building);
            //Una vez agregado el edificio la cantidad de veces correspondientes, lo saco de la lista y sigo con el resto.
            _keyBuildings.Remove(building);
        }
    }

    private void PlaceNormalBuildings()
    {
        while (_normalBuildings.Count > 0)
        {
            var building = _normalBuildings[_normalBuildings.Count];
            //Si el edificio puede repetirse en la ciudad lo ubica la cantidad de veces que se configuro en cada ciudad.
            for (int i = 0; i < building.Settings.AmountPerCity; i++)
                foreach (var item in _cities)
                    PlaceBuildingInCity(building, item);

            //Una vez agregado el edificio la cantidad de veces correspondientes, lo saco de la lista y sigo con el resto.
            _normalBuildings.Remove(building);
        }
    }

    /// <summary>
    /// Coloca un edificio en un lugar aleatorio del mapa.
    /// </summary>
    private void PlaceBuildingInWorld(Building building)
    {
        //Ordeno las ciudades
        OrderCities();

        //Ubico en una ciudad aleatoria el edificio ignorando la que mas KeyBuildings tiene.
        PlaceBuildingInCity(building, _cities[Random.Range(1, _cities.Count - 1)]);
    }

    /// <summary>
    /// Coloca un edificio en un lugar aleatorio de una ciudad.
    /// </summary>
    private void PlaceBuildingInCity(Building building, City city)
    {
        foreach (var item in city.Blocks)
            if (item.AvaliableSpace >= building.TotalTileSize) item.AddBuilding(building);
    }

    private void OrderCities() => _cities.OrderBy(x => x.AmountOfKeyBuildings);

    #endregion
}
