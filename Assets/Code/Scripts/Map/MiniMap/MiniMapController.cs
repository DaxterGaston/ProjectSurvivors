using UnityEngine;
using UnityEngine.UI; 

public class MiniMapController : MonoBehaviour
{
    #region Serializables

    [SerializeField] private float _heightAbovePlayer = 10f; // Altura de la cámara por encima del jugador
    [SerializeField] private Text[] _distanceTexts; // Array de Text para mostrar las distancias

    #endregion

    #region Miembros privados

    private GameObject[] _mapIcons;
    private Transform _playerTransform;

    #endregion

    #region Eventos de Unity

    private void Start()
    {
        _playerTransform = GameManager.Instance.PlayerManager.PlayerTransform;
        // Encuentra todos los objetos con el tag "MapUI"
        _mapIcons = GameObject.FindGameObjectsWithTag("MapUI");

        // Verifica si el array de textos tiene suficiente espacio para todos los iconos encontrados
        if (_distanceTexts.Length < _mapIcons.Length)
        {
            Debug.LogWarning("No hay suficientes elementos de texto para todos los iconos de MapUI.");
        }
    }

    private void LateUpdate()
    {
        if (_playerTransform == null)
        {
            Debug.LogWarning("Player Transform not set in MiniMapController.");
            return;
        }

        // Seguir la posición del jugador
        Vector3 newPosition = _playerTransform.position;
        newPosition.z = -_heightAbovePlayer; // Ajusta la altura de la cámara sobre el jugador
        transform.position = newPosition;

        // Seguir la rotación del jugador
        transform.rotation = _playerTransform.rotation;

        // Calcular y mostrar la distancia a cada icono
        UpdateDistances();
    }

    #endregion

    #region Metodos privados

    private void UpdateDistances()
    {
        for (int i = 0; i < _mapIcons.Length; i++)
        {
            float distance = Vector3.Distance(_playerTransform.position, _mapIcons[i].transform.position);

            // Verifica si hay un elemento de texto correspondiente
            if (i < _distanceTexts.Length)
            {
                // Muestra la distancia en el texto UI correspondiente
                _distanceTexts[i].text = $"{_mapIcons[i].name}: {distance:F2} metros";
            }
        }
    }

    #endregion
}