 using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class ControlIA : MonoBehaviour {
    public GameObject my_pionblack; // Contient le pion noir
    public GameObject my_pionwhite; // Contient le pion blanc
    public float case_size = 0.40f; // Taille d'une case
	public Sprite blackCross;
	public Sprite whiteCross;
    public Sprite redCross;
    public resultatIA result;
    public int player = 1;
    public int playersave = 0;
    public int playerwin;
    public menurules rules;
    public bool b_three = false;
    bool isPaused = false;
                            
    private int nb_patt = 6;
    private string[] patt = new string[6] { "01110", "010110", "011010", "10110", "11010", "011110" };

    private Rigidbody2D my_rig; // Notre RigidBody
    private Vector2 position; // Position temporaire
	private SpriteRenderer spriteRenderer; 

    private int[,]          tab = new int[19, 19];
    private GameObject[,]   tabObject = new GameObject[19, 19];
    private int p_x;
    private int p_y;

    private int black_score = 0;
    private int white_score = 0;

    public Text countTextBlack;
    public Text countTextWhite;
    public Text Winner;

    bool isAttack;
    private int[] last_pawn = new int[2];


    float recalc(float pos) // Calcul et arrondis en fonctions de la position de base du curseur
    {
        float npos = pos;
        if (npos % case_size >= case_size/2)
            npos += case_size - (npos % case_size);
        else if (npos > 0)
            npos -= (npos % case_size);
        else if (npos % case_size <= -case_size/2)
            npos -= case_size + (npos % case_size);
        else
            npos -= (npos % case_size);
        if (npos >= case_size * 9)
            npos = case_size * 9;
        else if (npos <= -case_size * 9)
            npos = -case_size * 9;
        return npos;
    }

    int where_in_tab(float pos) // retourne la case correspondante a une coordonnée x ou y
    {
        float place = pos / case_size;
        if (place > 0)
            place += 9;
        else if (place < 0)
            place = 9 + place;
        else
            place = 9;
        return (int)place;
    }

    int calc_pos(int player) // Verifie l'absence de jeton dans le tableau puis en ajoute un si possible ou retourne le jeton actuelle
	{
        int j;
        int i;

        j = where_in_tab(position.y);
        i = where_in_tab(position.x);
        if (tab[j, i] == 0)
        {
            tab[j, i] = player;
            tabObject[j, i] = Instantiate(my_pionblack, my_rig.position, new Quaternion());
            VerifCaptured(j, i);
            Debug.Log("Player joue en ["+j+"] ["+i+"]");
            last_pawn[0] = j;
            last_pawn[1] = i;
            return (0);
        }
        return (tab[j,i]);

    }

    int verif_line()
    {
        int i = 0, j;
        int stack = 0;
        int flag = 0;
        while (i < 19)
        {	
            j = 0;
            while (j < 19)
            {
                if (tab[i,j] != 0)
                {
                    if (tab[i, j] == flag)
                    {
                        stack++;
                        if (stack == 5)
                            return (tab[i, j]);
                    }
                    else
                    {
                        stack = 1;
                        flag = tab[i, j];
                    }
                }
                else
                {
                    stack = 0;
                    flag = 0;
                }
                j++;
            }
            i++;
        }
        return (0);
    }

    int verif_col()
    {
        int i, j=0;
        int stack = 0;
        int flag = 0;
        while (j < 19)
        {
            i = 0;
            while (i < 19)
            {
                if (tab[i, j] != 0)
                {
                    if (tab[i, j] == flag)
                    {
                        stack++;
                        if (stack == 5)
                            return (tab[i, j]);
                    }
                    else
                    {
                        stack = 1;
                        flag = tab[i, j];
                    }
                }
                else
                {
                    stack = 0;
                    flag = 0;
                }
                i++;
            }
            j++;
        }
        return (0);
    }

    int verif_diag1()
    {
        int i_s = 4, j_s = 0;
        int flag = 0, stack = 0;
        int i, j;
        while (j_s < 19)
        {
            i = i_s;
            j = j_s;
            while (i >= 0 && j <= 18)
            {
                if (tab[i, j] != 0)
                {
                    if (tab[i, j] == flag)
                    {
                        stack++;
                        if (stack == 5)
                            return (tab[i, j]);
                    }
                    else
                    {
                        stack = 1;
                        flag = tab[i, j];
                    }
                }
                else
                {
                    stack = 0;
                    flag = 0;
                }
                i--;
                j++;
            }
            stack = 0;
            flag = 0;
            if (i_s < 18)
                i_s++;
            else
                j_s++;
        }
        return (0);
    }

    int verif_diag2()
    {
        int i_s = 4, j_s = 18;
        int flag = 0, stack = 0;
        int i, j;
        while (j_s > 0)
        {
            i = i_s;
            j = j_s;
            while (i >= 0 && j >= 0)
            {
                if (tab[i, j] != 0)
                {
                    if (tab[i, j] == flag)
                    {
                        stack++;
                        if (stack == 5)
                            return (tab[i, j]);
                    }
                    else
                    {
                        stack = 1;
                        flag = tab[i, j];
                    }
                }
                else
                {
                    stack = 0;
                    flag = 0;
                }
                i--;
                j--;
            }
            stack = 0;
            flag = 0;
            if (i_s < 18)
                i_s++;
            else
                j_s--;
        }
        return (0);
    }

    int verif_win()
    {
        int res;
        if ((res = verif_line()) != 0
            || (res = verif_col()) != 0
            || (res = verif_diag1()) != 0
            || (res = verif_diag2()) != 0
            || black_score >= 10 || white_score >= 10)
        {
            res = player;
            playerwin = player;
            isPaused = true;
            Cursor.visible = true;
            result.gameObject.SetActive(true);
            return (res);
        }
        return (0);
    }

    public void Start () {
        Cursor.visible = true;

        player = Random.Range(1, 3);

        // Sélection de stratégie de l'IA
        isAttack = (player == 1 ? false : true);

        spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer.sprite == null)
			spriteRenderer.sprite = blackCross;

        countTextBlack.text = "0/10";
        countTextWhite.text = "0/10";
    }

    private void     VerifCaptured(int j, int i)
    {
        int eaten;

        eaten = (player == 1 ? 2 : 1);

        VerifCapturedLineVertical(j, i, eaten);
        VerifCapturedLineHorizontal(j, i, eaten);
        VerifCapturedDiagonal(j, i, eaten);
        VerifCapturedDiagonal2(j, i, eaten);
        UpdateScoreText();
    }

    private void    VerifCapturedLineHorizontal(int j, int i, int eaten) {
        // Check at left of pawn
        if (i >= 3 && tab[j, i - 3] == player && tab[j, i - 2] == eaten && tab[j, i - 1] == eaten)
        {
            // Clean both arrays
            tab[j, i - 2] = 0;
            tab[j, i - 1] = 0;
            Destroy(tabObject[j, i - 2]);
            Destroy(tabObject[j, i - 1]);

            AddScore();
        }

        // Check at right of pawn
        if (i <= 15 && tab[j, i + 3] == player && tab[j, i + 1] == eaten && tab[j, i + 2] == eaten)
        {
            // Clean both arrays
            tab[j, i + 1] = 0;
            tab[j, i + 2] = 0;
            Destroy(tabObject[j, i + 1]);
            Destroy(tabObject[j, i + 2]);

            AddScore();
        }
    }

    private void VerifCapturedLineVertical(int j, int i, int eaten)
    {
        // Check at bot of pawn
        if (j >= 3 && tab[j - 3, i] == player && tab[j - 2, i] == eaten && tab[j - 1, i] == eaten)
        {
            // Clean both arrays
            tab[j - 2, i] = 0;
            tab[j - 1, i] = 0;
            Destroy(tabObject[j - 2, i]);
            Destroy(tabObject[j - 1, i]);

            AddScore();
        }

        // Check at top of pawn
        if (j <= 15 && tab[j + 3, i] == player && tab[j + 1, i] == eaten && tab[j + 2, i] == eaten)
        {
            // Clean both arrays
            tab[j + 1, i] = 0;
            tab[j + 2, i] = 0;
            Destroy(tabObject[j + 1, i]);
            Destroy(tabObject[j + 2, i]);

            AddScore();
        }
    }

    /* Check:
    **              X
    **          O  
    **      O
    **  X
    */
    private void    VerifCapturedDiagonal(int j, int i, int eaten)
    {
        if (j >= 3 && i >= 3 && tab[j - 3, i - 3] == player && tab[j - 2, i - 2] == eaten && tab[j - 1, i - 1] == eaten)
        {
            tab[j - 1, i - 1] = 0;
            tab[j - 2, i - 2] = 0;
            Destroy(tabObject[j - 1, i - 1]);
            Destroy(tabObject[j - 2, i - 2]);

            AddScore();
        }

        if (j <= 15 && i <= 15 && tab[j + 3, i + 3] == player && tab[j + 2, i + 2] == eaten && tab[j + 1, i + 1] == eaten)
        {
            tab[j + 1, i + 1] = 0;
            tab[j + 2, i + 2] = 0;
            Destroy(tabObject[j + 1, i + 1]);
            Destroy(tabObject[j + 2, i + 2]);

            AddScore();
        }
    }


    /* Check:
    **      X       
    **          O  
    **              O
    **                  X
    */
    private void VerifCapturedDiagonal2(int j, int i, int eaten)
    {
        if (j <= 15 && i >= 3 && tab[j + 3, i - 3] == player && tab[j + 2, i - 2] == eaten && tab[j + 1, i - 1] == eaten)
        {
            tab[j + 1, i - 1] = 0;
            tab[j + 2, i - 2] = 0;
            Destroy(tabObject[j + 1, i - 1]);
            Destroy(tabObject[j + 2, i - 2]);

            AddScore();
        }

        if (j >= 3 && i <= 15 && tab[j - 3, i + 3] == player && tab[j - 2, i + 2] == eaten && tab[j - 1, i + 1] == eaten)
        {
            tab[j - 1, i + 1] = 0;
            tab[j - 2, i + 2] = 0;
            Destroy(tabObject[j - 1, i + 1]);
            Destroy(tabObject[j - 2, i + 2]);

            AddScore();
        }
    }

    private void AddScore()
    {
        if (player == 1)
            black_score += 2;
        else
            white_score += 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (my_rig == null)
            my_rig = GetComponent<Rigidbody2D>();
        position = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        position.x = recalc(position.x);
        position.y = recalc(position.y);
        my_rig.position = position;
        CheckSelectorSprite();
        playersave = player;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            rules.gameObject.SetActive(true);
        }

        if (isPaused == false)
        {
            //print("Player: " + player);
            rules.gameObject.SetActive(false);
            Time.timeScale = 1;
            if (player == 2)
                findMove();
            if (Input.GetMouseButtonDown(0)) {
                if (b_three == false || IsThree(position.x, position.y) < 2) {
                    if (calc_pos(player) == 0) {
                        verif_win();
                        player = 2;
                    }
                }
            }
        }
        else
           Time.timeScale = 0;
    }

    public void Checkpaused() {
        if (isPaused == true)
            isPaused = false;
        else
            isPaused = true;
    }

    void CheckSelectorSprite() {
		int j;
		int i;

		j = where_in_tab(position.y);
		i = where_in_tab(position.x);
        if (isPaused)
            spriteRenderer.sprite = null;
        else if (tab[j, i] != 0)
            spriteRenderer.sprite = redCross;
        else
            spriteRenderer.sprite = blackCross;
	}

    private void    UpdateScoreText()
    {
        countTextBlack.text = black_score.ToString() + "/10";
        countTextWhite.text = white_score.ToString() + "/10";
    }


    // REGLE DE TROIS
    //
    //
    // Cherche les differents pattern
    int DetectPatt(string tab)
    {
        int i = 0;

        while (i < this.nb_patt)
        {
            if (tab.Contains(this.patt[i]))
            {
                print("Double trois trouvé! -> " + tab);
                return (1);
            }
            i++;
        }
        return 0;
    }

    // conversion ligne
    int ThreeInLine(int x, int y)
    {
        char[] tmptab = new char[7];
        int j = y - 3;
        int i = 0;

        while (j < y + 4 && j < 19)
        {
            if (j >= 0 && this.tab[x, j] == player || j == y)
                tmptab[i] = '1';
            else if (j >= 0 && this.tab[x, j] == 0)
                tmptab[i] = '0';
            else
                tmptab[i] = '2';
            i++;
            j++;
        }
        while (i < 7)
            tmptab[i++] = '2';
        string res = new string(tmptab);
        print("Line -> " + res);
        return DetectPatt(new string(tmptab));
    }

    // conversion colonne
    int ThreeInCol(int x, int y)
    {
        char[] tmptab = new char[7];
        int j = x - 3;
        int i = 0;

        while (j < x + 4 && j < 19)
        {
            if (j >= 0 && this.tab[j, y] == player || j == x)
                tmptab[i] = '1';
            else if (j >= 0 && this.tab[j, y] == 0)
                tmptab[i] = '0';
            else
                tmptab[i] = '2';
            i++;
            j++;
        }
        while (i < 7)
            tmptab[i++] = '2';
        string res = new string(tmptab);
        print("Colonne -> " + res);
        return DetectPatt(new string(tmptab));
    }

    //converstion diagonale
    int ThreeInDiag(int x, int y)
    {
        char[] tmptab = new char[7];
        int j = x;
        int i = y;
        int compt = 0;

        while (compt < 3 && j > 0 && i > 0)
        {
            j--;
            i--;
            compt++;
        }
        compt = 0;
        while (j < x + 4 && i < y + 4 && j < 19 && i < 19)
        {
            if (this.tab[j, i] == player || (j == x && i == y))
                tmptab[compt] = '1';
            else if (this.tab[j, i] == 0)
                tmptab[compt] = '0';
            else
                tmptab[compt] = '2';
            i++;
            j++;
            compt++;
        }
        while (compt < 7)
            tmptab[compt++] = '2';
        string res = new string(tmptab);
        print("Diag -> " + res);
        return DetectPatt(new string(tmptab));
    }

    //converstion diagonale2
    int ThreeInDiag2(int x, int y)
    {
        char[] tmptab = new char[7];
        int j = x;
        int i = y;
        int compt = 0;

        while (compt < 3 && j > 0 && i < 18)
        {
            j--;
            i++;
            compt++;
        }
        compt = 0;
        while (j < x + 4 && i > y - 4 && j < 19 && i >= 0)
        {
            if (this.tab[j, i] == player || (j == x && i == y))
                tmptab[compt] = '1';
            else if (this.tab[j, i] == 0)
                tmptab[compt] = '0';
            else
                tmptab[compt] = '2';
            i--;
            j++;
            compt++;
        }
        while (compt < 7)
            tmptab[compt++] = '2';
        string res = new string(tmptab);
        print("Diag2 -> " + res);
        return DetectPatt(new string(tmptab));
    }

    int IsThree(float a, float b)
    {
        int res = 0;
        int x = where_in_tab(b);
        int y = where_in_tab(a);

        res = ThreeInLine(x, y);
        res = res + ThreeInCol(x, y);
        res = res + ThreeInDiag(x, y);
        res = res + ThreeInDiag2(x, y);
        print("Total -> " + res);
        return res;
    }

    int    findMove()
    {        
        if (isAttack == false) // boucle défensive
        {
            print("Tour de l'IA. Boucle défensive");
            if (IA_CanWin() ||          // Si on peut gagner en une fois, poser le pion de la win
                IA_ennemyCanWin() ||    // Si on peut perdre en une fois, poser le pion pour empêcher la win adverse
                IA_blockEnnemy(3) ||    // Si l'ennemi a un alignement de 3, on bloque
                IA_canEatEnnemy() ||    // Si on peut manger
                IA_blockEnnemy(2))      // Si l'ennemi a un alignement de 2, on bloque
                return 0;

            // On a rien joué, donc on passe en mode attaque et on rappelle findMove();
            // Décommenter pour tester partie offensive    

            //isAttack = true;
            //findMove();
            //print("Rien trouvé, on passe en mode aggro");
        }
        else // boucle offensive
        {
            print("Tour de l'IA. Boucle offensive");
            isAttack = false;
        }
        return (0);
    }

    bool IA_Play(int x, int y)
    {
        print("IA joue en [" + x + "] [" + y + "]");
        tab[y, x] = 2;
        tabObject[y, x] = Instantiate(my_pionwhite, my_rig.position, new Quaternion()); // Recaculer my_rig.position par rapport à y x
        VerifCaptured(y, x);
        verif_win();
        player = 1;
        return true;
    }

    bool     IA_CanWin()
    {
        int j = 0;
        int i;

        while (j < 19)
        {
            i = 0;
            while (i < 19)
            {
                if (tab[j, i] == 2 && tab[j, i + 1] == 2 && tab[j, i + 2] == 2 && tab[j, i + 3] == 2) // horizontal  O X X X X O 
                {
                    if (i > 0 && tab[j, i - 1] == 0)
                        return IA_Play(i - 1, j);
                    else if (i < 15 && tab[j, i + 4] == 0)
                        return IA_Play(i + 4, j);
                }
                else if (tab[j, i] == 2 && tab[j, i + 1] == 0 && tab[j, i + 2] == 2 && tab[j, i + 3] == 2 && tab[j, i + 4] == 2) // X O X X X
                    return IA_Play(i + 1, j);
                else if (tab[j, i] == 2 && tab[j, i + 1] == 2 && tab[j, i + 2] == 0 && tab[j, i + 3] == 2 && tab[j, i + 4] == 2) // X X O X X
                    return IA_Play(i + 2, j);
                else if (tab[j, i] == 2 && tab[j, i + 1] == 2 && tab[j, i + 2] == 2 && tab[j, i + 3] == 0 && tab[j, i + 4] == 2) // X X X O X
                    return IA_Play(i + 3, j);

                else if (tab[j, i] == 2 && tab[j + 1, i] == 2 && tab[j + 2, i] == 2 && tab[j + 3, i] == 2) // vertical
                {
                    if (j > 0 && tab[j - 1, i] == 0)
                        return IA_Play(i, j - 1);
                    else if (j < 15 && tab[j + 4, i] == 0)
                        return IA_Play(i, j + 4);
                }
                else if (tab[j, i] == 2 && tab[j + 1, i] == 0 && tab[j + 2, i] == 2 && tab[j + 3, i] == 2 && tab[j + 4, i] == 2)
                    return IA_Play(i, j + 1);
                else if (tab[j, i] == 2 && tab[j + 1, i] == 2 && tab[j + 2, i] == 0 && tab[j + 3, i] == 2 && tab[j + 4, i] == 2)
                    return IA_Play(i, j + 2);
                else if (tab[j, i] == 2 && tab[j + 1, i] == 2 && tab[j + 2, i] == 2 && tab[j + 3, i] == 0 && tab[j + 4, i] == 2)
                    return IA_Play(i, j + 3);

                else if (i > 3 && tab[j, i] == 2 && tab[j + 1, i - 1] == 2 && tab[j + 2, i - 2] == 2 && tab[j + 3, i - 3] == 2) // diago no se
                {
                    if (j > 0 && tab[j - 1, i + 1] == 0)
                        return IA_Play(i + 1, j - 1);
                    else if (j < 15 && tab[j + 4, i - 4] == 0)
                        return IA_Play(i - 4, j + 4);
                }
                else if (i > 3 && tab[j, i] == 2 && tab[j + 1, i - 1] == 0 && tab[j + 2, i - 2] == 2 && tab[j + 3, i - 3] == 2 && tab[j + 4, i - 4] == 2)
                    return IA_Play(i - 1, j + 1);
                else if (i > 3 && tab[j, i] == 2 && tab[j + 1, i - 1] == 2 && tab[j + 2, i - 2] == 0 && tab[j + 3, i - 3] == 2 && tab[j + 4, i - 4] == 2)
                    return IA_Play(i - 2, j + 2);
                else if (i > 3 && tab[j, i] == 2 && tab[j + 1, i - 1] == 2 && tab[j + 2, i - 2] == 2 && tab[j + 3, i - 3] == 0 && tab[j + 4, i - 4] == 2)
                    return IA_Play(i - 3, j + 3);

                else if (i < 15 && tab[j, i] == 2 && tab[j + 1, i + 1] == 2 && tab[j + 2, i + 2] == 2 && tab[j + 3, i + 3] == 2) // diago ne so
                {
                    if (j > 0 && tab[j - 1, i - 1] == 0)
                        return IA_Play(i - 1, j - 1);
                    else if (j < 15 && tab[j + 4, i + 4] == 0)
                        return IA_Play(i + 4, j + 4);
                }
                else if (i < 15 && tab[j, i] == 2 && tab[j + 1, i + 1] == 0 && tab[j + 2, i + 2] == 2 && tab[j + 3, i + 3] == 2 && tab[j + 4, i + 4] == 2)
                    return IA_Play(i + 1, j + 1);
                else if (i < 15 && tab[j, i] == 2 && tab[j + 1, i + 1] == 2 && tab[j + 2, i + 2] == 0 && tab[j + 3, i + 3] == 2 && tab[j + 4, i + 4] == 2)
                    return IA_Play(i + 2, j + 2);
                else if (i < 15 && tab[j, i] == 2 && tab[j + 1, i + 1] == 2 && tab[j + 2, i + 2] == 2 && tab[j + 3, i + 3] == 0 && tab[j + 4, i + 4] == 2)
                    return IA_Play(i + 3, j + 3);
                i++;
            }
            j++;
        }
        print("Sortie boucle IA_CanWin");
        return (false);
    }



    bool    IA_ennemyCanWin()
    {

        return (false);
    }

    bool    IA_blockEnnemy(int nb)
    {

        return (false);
    }

    bool    IA_canEatEnnemy()
    {

        return (false);
    }
}
