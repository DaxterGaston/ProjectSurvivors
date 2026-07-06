using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MapNode
{
    [SerializeField]
    protected BlockMapNodeSO _settings;

    private int[,] _blockMap;

    public int AvaliableSpace { get; private set; }

    private void Awake()
    {
        _blockMap = new int[_settings.Width, _settings.Height];
        AvaliableSpace = _settings.Width * _settings.Height;
    }

    public BlockMapNodeSO Settings { get => _settings; protected set { _settings = value; } }

    /// <summary>
    /// Agrega el edificio a la manzana.
    /// </summary>
    /// <param name="building"></param>
    /// <returns>True si agrego el edificio a la manzana. - False si no habia espacio para agregarlo.</returns>
    public bool AddBuilding(Building building)
    {
        //if (CheckForSpace(building) >= 0)
        //    Children.Add(building);
        return true;
    }

    #region Metodos Privados

    private int CheckForSpace(Building building) => (AvaliableSpace > building.TotalTileSize) ? CheckIfFits(building) : -1;

    private int CheckIfFits(Building building)
    {
        switch (building.Settings.Door)
        {
            case DoorSide.Top: 
                return CheckIfFitsTop(building.Settings.WidthOnBlock);
            case DoorSide.Left:
                return CheckIfFitsLeft(building.Settings.HeightOnBlock);
            case DoorSide.Right:
                return CheckIfFitsRight(building.Settings.HeightOnBlock);
            case DoorSide.Bottom:
                return CheckIfFitsBottom(building.Settings.WidthOnBlock);
        }
        throw new ArgumentException();
    }

    private int CheckIfFitsTop(int size)
    {
        int row = 0;
        //Si es un edificio de 1 tile de ancho devuelvo la primer posicion que tenga 0
        if (size == 1)
            for (int i = 0; i < _settings.Width; i++)
                if (_blockMap[row, i] == 0) return i;

        //Si el edificio es de mas de 1 tile de ancho recorro posiciones hasta encontrar 0 y a partir de ahi el ancho.
        int aux = 0;
        for (int i = 0; i < _settings.Width; i++)
        {
            if (_blockMap[row, i] == 0)
            {
                for (int j = 0; j < _settings.Width - i; j++)
                    aux += _blockMap[row, j];
                if (aux == 0)
                    return i;        
            }
        }
        return -1;
    }

    private int CheckIfFitsBottom(int size)
    {
        int row = _settings.Height -1;
        //Si es un edificio de 1 tile de ancho devuelvo la primer posicion que tenga 0
        if (size == 1)
            for (int i = 0; i < _settings.Width; i++)
                if (_blockMap[row, i] == 0) return i;

        //Si el edificio es de mas de 1 tile de ancho recorro posiciones hasta encontrar 0 y a partir de ahi el ancho.
        int aux = 0;
        for (int i = 0; i < _settings.Width; i++)
        {
            if (_blockMap[row, i] == 0)
            {
                for (int j = 0; j < _settings.Width - i; j++)
                    aux += _blockMap[row, j];
                if (aux == 0)
                    return i;
            }
        }
        return -1;
    }

    [Obsolete("Se deja obsoleto hasta finalizar otras tareas.")]
    private int CheckIfFitsLeft(int size)
    {
        //for (int i = 0; i < _settings.Height; i++)
        //{

        //}
        throw new NotImplementedException();
    }

    [Obsolete("Se deja obsoleto hasta finalizar otras tareas.")]
    private int CheckIfFitsRight(int size)
    {
        //for (int i = 0; i < _settings.Height; i++)
        //{

        //}
        throw new NotImplementedException();
    }
    #endregion
}
