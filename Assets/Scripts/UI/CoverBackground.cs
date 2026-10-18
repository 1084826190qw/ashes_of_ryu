using UnityEngine;
using UnityEngine.UI;

namespace AshesOfRyu
{
    /// <summary>
    /// Hace que una imagen de fondo llene su padre SIN deformarse (cover) y permite
    /// elegir qué borde vertical se ve y cuál se recorta.
    /// Pensado para el fondo del menú: la imagen trae el título del juego arriba,
    /// así que con <see cref="verticalAlign"/> = 1 se muestra el borde superior
    /// (el título) y se recorta por abajo.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class CoverBackground : MonoBehaviour
    {
        [Header("Encuadre vertical")]
        [Tooltip("1 = muestra el borde SUPERIOR (recorta abajo). 0.5 = centrado. 0 = muestra el borde INFERIOR (recorta arriba).")]
        [Range(0f, 1f)]
        [SerializeField] private float verticalAlign = 1f;

        [Tooltip("Desplazamiento vertical extra (en píxeles de referencia). Negativo = baja más la imagen.")]
        [SerializeField] private float offsetY = 0f;

        private RectTransform _rect;

        private RectTransform Rect
        {
            get
            {
                if (_rect == null)
                    _rect = (RectTransform)transform;
                return _rect;
            }
        }

        private void OnEnable() => Apply();
        private void OnValidate() => Apply();
        private void OnRectTransformDimensionsChange() => Apply();

        /// <summary>
        /// Calcula tamaño y posición para cubrir el padre manteniendo la proporción
        /// del sprite, alineando la imagen según <see cref="verticalAlign"/>.
        /// </summary>
        public void Apply()
        {
            RectTransform rect = Rect;
            if (rect == null)
                return;

            RectTransform parent = rect.parent as RectTransform;
            if (parent == null)
                return;

            Image image = GetComponent<Image>();
            if (image == null || image.sprite == null)
                return;

            Vector2 parentSize = parent.rect.size;
            if (parentSize.x <= 0f || parentSize.y <= 0f)
                return;

            float spriteAspect = image.sprite.rect.width / image.sprite.rect.height;

            float width, height;
            if (parentSize.x / parentSize.y > spriteAspect)
            {
                // El padre es más ancho que la imagen: se ajusta al ancho y sobra alto.
                width = parentSize.x;
                height = parentSize.x / spriteAspect;
            }
            else
            {
                // El padre es más alto: se ajusta al alto y sobra ancho.
                height = parentSize.y;
                width = parentSize.y * spriteAspect;
            }

            float overflowY = height - parentSize.y;
            Vector2 newSize = new Vector2(width - parentSize.x, overflowY);
            Vector2 newPosition = new Vector2(0f, overflowY * (0.5f - verticalAlign) + offsetY);
            Vector2 centeredPivot = new Vector2(0.5f, 0.5f);

            if (rect.anchorMin != Vector2.zero) rect.anchorMin = Vector2.zero;
            if (rect.anchorMax != Vector2.one) rect.anchorMax = Vector2.one;
            if (rect.pivot != centeredPivot) rect.pivot = centeredPivot;
            if (rect.sizeDelta != newSize) rect.sizeDelta = newSize;
            if (rect.anchoredPosition != newPosition) rect.anchoredPosition = newPosition;
        }
    }
}
