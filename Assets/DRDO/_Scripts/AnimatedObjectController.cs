using System.Collections.Generic;
using UnityEngine;

public class AnimatedObjectController : MonoBehaviour
{
    public List<GameObject> animatedPrefabs; // Assign animated prefabs in Inspector
    private RuntimeAnimatorController defaultController;

    private void Start()
    {
        // Load the default Animator Controller from Resources
        defaultController = Resources.Load<RuntimeAnimatorController>("DefaultAnimator");

        // Assign animations to all prefabs dynamically
        foreach (GameObject prefab in animatedPrefabs)
        {
            AssignAnimationClip(prefab);
        }
    }

    public void AssignAnimationClip(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("❌ Prefab is NULL!");
            return;
        }

        // 🔹 Get Animator Component
        Animator anim = prefab.GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError($"❌ Animator is missing on {prefab.name}. Please add it manually.");
            return;
        }

        // 🔹 Find Animation Clip inside prefab or its children
        AnimationClip foundClip = FindAnimationClip(prefab);
        if (foundClip == null)
        {
            Debug.LogError($"❌ No Animation Clip found in {prefab.name}");
            return;
        }

        // 🔹 Assign an Animator Override Controller to the prefab
        anim.runtimeAnimatorController = CreateOverrideController(foundClip);

        // 🔹 Play the animation
        anim.Play(foundClip.name);

        Debug.Log($"✅ Animation '{foundClip.name}' assigned & playing on {prefab.name}");
    }

    private AnimationClip FindAnimationClip(GameObject prefab)
    {
        // Check in Animation Component (if exists)
        Animation animation = prefab.GetComponentInChildren<Animation>();
        if (animation != null && animation.clip != null)
        {
            return animation.clip; // Return found clip
        }

        // Check if there's an Animator and retrieve clip from its Controller
        Animator animator = prefab.GetComponentInChildren<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller.animationClips.Length > 0)
            {
                return controller.animationClips[0]; // Return first clip
            }
        }

        return null; // No animation found
    }

    private RuntimeAnimatorController CreateOverrideController(AnimationClip clip)
    {
        // 🔹 Create an Animator Override Controller and override the clip
        AnimatorOverrideController overrideController = new AnimatorOverrideController(defaultController);
        overrideController[clip.name] = clip; // Assign animation clip dynamically

        return overrideController;
    }
}
