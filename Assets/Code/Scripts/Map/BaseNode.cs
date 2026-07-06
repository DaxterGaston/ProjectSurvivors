using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define la jerarquia del nodo.
/// Finalidad de performance, es mejor guardar este valor en cada nodo que tener que hacer un typeof en ejecucion
/// ya que las colecciones en las que se guarda el grafo admiten todos como BaseNode sin discriminar por su tipo.
/// </summary>
public enum NodeHierarchy
{
    Primary, //Mapa
    Secondary, //Ciudad / Naturaleza
    Tertiary, //Manzana / Bosque / Pantano / Lago
    Quaternary //Edificio
}

public abstract class BaseNode : MonoBehaviour
{
    public int NodeHeight { get; protected set; }
    public int NodeWidth { get; protected set; }
    /// <summary>
    /// Lista de hijos (un nodo solo puede tener hijos de menor jerarquia).
    /// </summary>
    protected List<KeyValuePair<BaseEdge, BaseNode>> Children { get; set; }
    public NodeHierarchy NodeHierarchy { get; protected set; }

    public void AddChild(BaseNode node)
    {
        Children.Add(new KeyValuePair<BaseEdge, BaseNode>(new BaseEdge(this, node), node));   
    }

    #region Debug

    private void OnDrawGizmos()
    {
        Vector3 treeRenderPos = transform.position;
        switch (NodeHierarchy)
        {
            case NodeHierarchy.Primary:
                Gizmos.color = Color.red;
                treeRenderPos.z = 1;
                break;
            case NodeHierarchy.Secondary:
                Gizmos.color = Color.green;
                treeRenderPos.z = 2;
                break;
            case NodeHierarchy.Tertiary:
                Gizmos.color = Color.blue;
                treeRenderPos.z = 3;
                break;
            case NodeHierarchy.Quaternary:
                Gizmos.color = Color.yellow;
                treeRenderPos.z = 4;
                break;
        }
        Gizmos.DrawWireSphere(transform.position, NodeWidth * .5f);

        if (Children.Count > 0) 
        {
            Gizmos.color = Color.magenta;
            foreach (var item in Children)
                Gizmos.DrawLine(transform.position, item.Value.transform.position);
        }
    }

    #endregion
}

public struct BaseEdge 
{
    public BaseEdge(BaseNode source, BaseNode end)
    {
        Source = source;
        End = end;
    }
    public BaseNode Source { get; private set; }
    public BaseNode End { get; private set; }
}

[CreateAssetMenu(fileName = "New block stats", menuName = "Scriptables/Block")]
public class BlockMapNodeSO : ScriptableObject
{
    #region Sets

    [SerializeField]
    protected int _children;
    [SerializeField]
    protected int _width;
    [SerializeField]
    protected int _height;

    #endregion

    #region Gets

    public int Children { get => _children; }
    public int Width { get => _width; }
    public int Height { get => _height; }

    #endregion
}

[CreateAssetMenu(fileName = "New building stats", menuName = "Scriptables/Building")]
public class BuildingMapNodeSO : ScriptableObject
{
    #region Sets

    [SerializeField]
    protected int _amountPerWorld;
    [SerializeField]
    protected int _amountPerCity;
    [SerializeField]
    protected bool _isKeyBuilding;
    [SerializeField]
    protected bool _isImportantBuilding;
    [SerializeField]
    [Range(1, 2)]
    /// <summary>Ancho que ocupa el edificio en la manzana.</summary>
    protected int _widthOnBlock;
    [SerializeField]
    [Range(1, 2)]
    ///<summary>Alto que ocupa el edificio en la manzana.</summary>
    protected int _heightOnBlock;
    [SerializeField]
    protected DoorSide _door;

    #endregion

    #region Gets

    public int AmountPerWorld => _amountPerWorld;
    public int AmountPerCity => _amountPerCity;
    public bool IsKeyBuilding => _isKeyBuilding;
    public bool IsImportantBuilding => _isImportantBuilding;
    public int WidthOnBlock => _widthOnBlock;
    public int HeightOnBlock => _heightOnBlock;
    public DoorSide Door => _door;

    #endregion
}