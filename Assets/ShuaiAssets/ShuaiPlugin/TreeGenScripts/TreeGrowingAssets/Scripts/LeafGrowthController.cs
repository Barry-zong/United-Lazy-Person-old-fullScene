using UnityEngine;
using System.Collections;

namespace TreeGrowingAssets.Scripts
{
    public class LeafGrowthController : MonoBehaviour
    {
        private Vector3 targetScale;
        private float currentGrowthTime = 0f;

        [Header("生长动画设置")]
        [Range(0.1f, 5f)]
        public float growthDuration = 2f; // 生长持续时间
        [Range(0f, 5f)]
        public float growthDelay = 2f; // 生长延迟时间

        [Header("大小随机设置")]
        [Range(0.5f, 1.5f)]
        public float minSizeMultiplier = 0.8f; // 最小尺寸倍数
        [Range(0.5f, 1.5f)]
        public float maxSizeMultiplier = 1.5f; // 最大尺寸倍数

        public void Initialize(Vector3 finalScale)
        {
            // 计算随机大小
            float randomMultiplier = Random.Range(minSizeMultiplier, maxSizeMultiplier);
            targetScale = finalScale * randomMultiplier;
            
            transform.localScale = Vector3.zero; // 初始大小为0
            StartCoroutine(GrowLeaf());
        }

        private IEnumerator GrowLeaf()
        {
            // 等待延迟时间
            yield return new WaitForSeconds(growthDelay);

            while (currentGrowthTime < growthDuration)
            {
                currentGrowthTime += Time.deltaTime;
                // 使用平滑的插值函数
                float t = currentGrowthTime / growthDuration;
                t = Mathf.SmoothStep(0, 1, t); // 使用平滑步进函数使动画更自然
                
                transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale; // 确保最终大小正确
            
            // 动画完成后销毁这个组件（但保留树叶对象）
            Destroy(this);
        }
    }
} 