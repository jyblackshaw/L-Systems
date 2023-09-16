using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LSystem : MonoBehaviour
{
    [SerializeField] GameObject debug_obj;
    public l_pattern pattern;
    [Range(0f, 180f)]
    [SerializeField] float displace_angle = 45f;

    //Note LSYSTEM, only allow single variable statements (leaf string & generation - **for now).

    float startAngle = 180f;
    float endAngle = 35f;
    float duration = 3f;

    public enum l_pattern
    {
        Leaf,
        Bushes_1,
        Bushes_2,
    }

    public bool draw_gizmos = false;

    private void Start()
    {
        draw_gizmos = true;
    }

    void OnDrawGizmos()
    {
        // Visualize L-System Pattern : Slider:
        Gizmos.color = Color.green;
        var leaf_str = Generate_Leaf_String("F", 5);
        Generate_Leaf_Pattern(leaf_str, displace_angle);
    }

    private string Generate_Leaf_String(string axiom, int iter_count)
    {
        //Recursive
        //Takes in String
        //Replace Rule
        //Each iteration modify string, if another iteration pass string to func, else return new string.
        //Depending if u want to keep branches the same size, will want to scale down segement, smaller the more iterations.

        if (iter_count == 0) { return axiom; }
        else
        {
            //Replace str according to rule (hard coded for now):

            string modified_str = "";
            // PATTERNS: 
            if (pattern == l_pattern.Leaf) modified_str = axiom.Replace("F", "F[+F]F[-F]F");
            else if (pattern == l_pattern.Bushes_1) modified_str = axiom.Replace("F", "FF+[+F-F-F]-[-F+F+F]");
            else if (pattern == l_pattern.Bushes_2) modified_str = axiom.Replace("F", "F[+FF][-FF]F[-F][+F]F");

            return Generate_Leaf_String(modified_str, iter_count - 1);
        }
    }

    //Generate_Leaf_String('F', F[+F]F[-F]F)
    private string Generate_Leaf_String_Generic(string chr_key, string replace_str, int iter_count)
    {
        if (iter_count == 0) { return replace_str; }
        else
        {
            //Replace str according to rule (hard coded for now):
            //Leaf formula: F = F[+F]F[-F]F
            string modified_str = replace_str.Replace(chr_key, replace_str);
            return Generate_Leaf_String_Generic(chr_key, modified_str, iter_count - 1);
        }
    }

    private enum branch_dir
    {
        straight,
        left,
        right
    }

    private IEnumerator Generate_Leaf_Pattern_Couroutine(string leaf_str, float displace_angle)
    {
        //NOTES:
        //F, [+F], [-F]
        //straight, left
        //multi nested brackets.
        //Each time we branch off we, need keep track of the branch location. Nested branches included. When we reach the inner most
        //branch (no longer nested branching), we go back..?

        print("leaf_str:" + leaf_str);

        //static variables:
        float l_segement_len = 10f;

        //changing variables:
        var branch_pos_angle_queue = new List<(Vector3, float)>();          //Keeps track of the positions before entering a branch & current angle at that time. Queue to keep track of nested branches.

        Vector3 current_pos = Vector3.zero;
        float current_angle = 90f;                            //angle calculated, relative to straight line.
        branch_dir branch_dir = branch_dir.straight;          //relative to current angle.
        int iteration = 0;

        List<Vector3> branch_point_order = new List<Vector3>();

        foreach (var chr in leaf_str)
        {
            yield return new WaitForSeconds(0);
            //START BRANCH:
            if (chr == '[')
            {
                branch_pos_angle_queue.Add((current_pos, current_angle));
                continue;
            }
            //END BRANCH:
            else if (chr == ']')
            {
                var branch_pos_angle = branch_pos_angle_queue[branch_pos_angle_queue.Count - 1];
                branch_pos_angle_queue.RemoveAt(branch_pos_angle_queue.Count - 1);
                current_pos = branch_pos_angle.Item1;
                current_angle = branch_pos_angle.Item2;
                branch_point_order.Add(current_pos);

                //GameObject.Instantiate(debug_obj, previous_pos, Quaternion.identity);

                continue;
            }
            else if (chr == '+')
            {
                //branch_dir = branch_dir.right;
                current_angle += displace_angle;
                continue;
            }
            else if (chr == '-')
            {
                branch_dir = branch_dir.left;
                current_angle -= displace_angle;
                continue;
            }
            else if (chr == 'F')
            {
                //update current_pos
                current_pos = get_point_on_circle(current_pos, current_angle, l_segement_len);
                branch_point_order.Add(current_pos);

                if (branch_dir == branch_dir.left || branch_dir == branch_dir.right)        //If segment preceded by Direction (L/R)
                {
                    //reset branch_dir to straight.
                    branch_dir = branch_dir.straight;
                }

                try
                {
                    var previous_pos = branch_point_order[branch_point_order.Count - 2];
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(previous_pos, current_pos);
                    //Gizmos.DrawLine(previous_pos, current_pos);

                }
                catch { /*print("debug did not work, branch_point_oder_len: " + branch_point_order.Count) ; */}

                iteration += 1;
            }
        }
    }

    private void Generate_Leaf_Pattern(string leaf_str, float displace_angle)
    {
        //NOTES:
        //F, [+F], [-F]
        //straight, left
        //multi nested brackets.
        //Each time we branch off we, need keep track of the branch location. Nested branches included. When we reach the inner most
        //branch (no longer nested branching).

        //static variables:
        float l_segement_len = 10f;

        //changing variables:
        var branch_pos_angle_queue = new List<(Vector3, float)>();          //Keeps track of the positions before entering a branch & current angle at that time. Queue to keep track of nested branches.

        Vector3 current_pos = Vector3.zero;
        float current_angle = 90f;                            //angle calculated, relative to straight line.
        branch_dir branch_dir = branch_dir.straight;          //relative to current angle.
        int iteration = 0;

        List<Vector3> branch_point_order = new List<Vector3>();

        foreach (var chr in leaf_str)
        {
            //START BRANCH:
            if (chr == '[')
            {
                branch_pos_angle_queue.Add((current_pos, current_angle));
                continue;
            }
            //END BRANCH:
            else if (chr == ']')
            {
                var branch_pos_angle = branch_pos_angle_queue[branch_pos_angle_queue.Count - 1];
                branch_pos_angle_queue.RemoveAt(branch_pos_angle_queue.Count - 1);
                current_pos = branch_pos_angle.Item1;
                current_angle = branch_pos_angle.Item2;
                branch_point_order.Add(current_pos);

                //GameObject.Instantiate(debug_obj, previous_pos, Quaternion.identity);

                continue;
            }
            else if (chr == '+')
            {
                //branch_dir = branch_dir.right;
                current_angle += displace_angle;
                continue;
            }
            else if (chr == '-')
            {
                branch_dir = branch_dir.left;
                current_angle -= displace_angle;
                continue;
            }
            else if (chr == 'F')
            {
                //update current_pos
                current_pos = get_point_on_circle(current_pos, current_angle, l_segement_len);
                branch_point_order.Add(current_pos);

                if (branch_dir == branch_dir.left || branch_dir == branch_dir.right)        //If segment preceded by Direction (L/R)
                {
                    //reset branch_dir to straight.
                    branch_dir = branch_dir.straight;
                }

                try
                {
                    var previous_pos = branch_point_order[branch_point_order.Count - 2];
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(previous_pos, current_pos);
                }
                catch { /*print("debug did not work, branch_point_oder_len: " + branch_point_order.Count);*/ }

                iteration += 1;
            }
        }
    }

    private Vector3 get_point_on_circle(Vector3 circle_center, float angle_degrees, float radius)
    {
        var radians = angle_degrees * Mathf.Deg2Rad;
        var x = Mathf.Cos(radians);
        var z = Mathf.Sin(radians);
        var pos = new Vector3(x, 0, z);
        pos = radius * pos;
        return circle_center + pos;
    }

    #region helper_funcs

    private IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    #endregion
}


