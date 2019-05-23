using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Matrix
{
    public float this[int r, int c] {
        set {
            Elements[r, c] = value;
        }
        get {
            return Elements[r, c];
        }
    }

    public int Row { get; private set; }
    public int Colmun { get; private set; }

    private float[,] Elements { get; set; }

    public Matrix(int row, int colmun) {
        Init(row, colmun);
    }

    public Matrix(float[] elements) {
        Init(1, elements.Length);
        for(int c = 0; c < Colmun; c++) {
            Elements[0, c] = elements[c];
        }
    }

    public void Init(int row, int colmun) {
        Row = row;
        Colmun = colmun;
        Elements = new float[row, colmun];
    }

    public Matrix Mul(Matrix m) {
        if(Colmun != m.Row) {
            throw new ArgumentException("colmun is not match row");
        }

        var newM = new Matrix(Row, m.Colmun);
        for(int r = 0; r < newM.Row; r++) {
            for(int c = 0; c < newM.Colmun; c++) {
                newM[r, c] = MulElement(m, r, c);
            }
        }
        return newM;
    }

    public Matrix Copy() {
        var m = new Matrix(Row, Colmun);
        for(int r = 0; r < Row; r++) {
            for(int c = 0; c < Colmun; c++) {
                m[r, c] = Elements[r, c];
            }
        }

        return m;
    }

    public float[] ToArray() {
        var list = new List<float>();
        for(int r = 0; r < Row; r++) {
            for(int c = 0; c < Colmun; c++) {
                list.Add(Elements[r, c]);
            }
        }
        return list.ToArray();
    }

    private float MulElement(Matrix m, int index1, int index2) {
        var v = 0.0f;
        for(int c = 0; c < Colmun; c++) {
            v += Elements[index1, c] * m[c, index2];
        }

        return v;
    }

    public override string ToString() {
        var str = "";
        for(int r = 0; r < Row; r++) {
            for(int c = 0; c < Colmun; c++) {
                str += Elements[r, c] + ",";
            }
            str += "\n";
        }

        return str;
    }
}
