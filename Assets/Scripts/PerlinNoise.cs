using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    [SerializeField] private float scale = 1.0f;
    [SerializeField] private int octaves = 3;
    [SerializeField] private float persistance = 0.5f;
    [SerializeField] private float lacunarity = 2;

    public float[,] GetNoise(float orgX, float orgY, int width, int height, int posX, int posY)
    {
        float[,] noiseMap = new float[width, height];
        scale = Mathf.Max(0.0001f, scale);
        float maxNoiseHeight = 1; //�ִ� ���� ��� ���� ����
        float minNoiseHeight = -1; //�ּ� ���� ��� ���� ����
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float amplitude = 1; //����. �������� ���� ���õ� ��.
                float frequency = 1; //���ļ�. �������� ���ݰ� ���õ� ��. ���ļ��� Ŀ������ ����� ��������
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) //��Ÿ�갡 �����Ҽ��� ���� ���ļ��� ���� ������ ����� ��ø��.
                {
                    float xCoord = orgX + (posX + x) / scale * frequency;
                    float yCoord = orgY + (posY + y) / scale * frequency;
                    float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1; //0~1 ������ ���� ��ȯ�ϴ� �Լ�. 2�� ���ϰ� 1�� ���� -1~1 ������ ������ ��ȯ
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                //if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                //else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;
                noiseMap[x, y] = noiseHeight;
            }
        }
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]); //lerp�� ���Լ��� �ּڰ��� �ִ��� ���հ��� 3��° ���ڷ� ������ 0~1������ ���� ��ȯ
            }
        }
        return noiseMap;
    }
}