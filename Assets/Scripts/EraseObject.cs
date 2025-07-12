using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraseObject : MonoBehaviour
{
	//�@����L�����N�^�[��Transform
	//private Transform characterTransform;
	private Renderer myRenderer;
	private Material[] materials;
	private Color[] colors_org;
	private Color[] colors_trans;
	//�@�I�u�W�F�N�g�̃A���t�@�l�𑀍삷��J�����Ƃ̋���
	[SerializeField]
	private float cameraDistance = 15.0f;
	// �J�����Ƃ̋����ɂ���ăA���t�@�l�ɉe����^����I�t�Z�b�g�l
	//[SerializeField]
	//private float alphaOffset = 5f;
	//�@�����𑪂�J����
	private Camera mainCamera;
	//�@�J�����ʒu�Ǝ��g�̈ʒu�̋���
	private float distance;
	//�@����L�����N�^�[�ƃJ�����̋���
	//private float distanceBetweenCharacterAndCamera;
	//�@�����_���[���J�����͈͓̔����ǂ���
	private bool isVisible;

	// Start is called before the first frame update
	void Start()
	{
		//characterTransform = GameObject.FindWithTag("Player").transform;
		mainCamera = Camera.main;
		myRenderer = GetComponent<Renderer>();
		materials = myRenderer.materials;

		colors_org = new Color[materials.Length];
		colors_trans = new Color[materials.Length];

		int idx = 0;

		foreach (var material in materials)
		{
			colors_org[idx] = material.color;
			colors_trans[idx] = new Color(material.color.r, material.color.g, material.color.b, 0.5f);
			idx++;
		}
	}

	// Update is called once per frame
	void Update()
	{
		//�@�J�����͈͓̔��łȂ���΂���ȍ~�̏������s��Ȃ�
		if (!isVisible)
		{
			return;
		}

		//�@�J�����ʒu�Ǝ��g�̈ʒu�̋������v�Z
		//distance = Vector3.Distance(mainCamera.transform.position, transform.position);
		distance = Vector3.Dot(transform.position-mainCamera.transform.position, mainCamera.transform.forward);

		//�@����L�����N�^�[�ƃJ�����̋���
		//distanceBetweenCharacterAndCamera = Vector3.Distance(mainCamera.transform.position, characterTransform.position);

		Debug.Log(distance);

		//�@����L�����N�^�[�ƃJ�����̊ԂɎ��g������ꍇ�A����cameraDistance���Z�������ɂ����ꍇ�͋����ɉ������A���t�@�l�ɂ���
		if (distance < cameraDistance)
		{
			int idx = 0;
			foreach (var material in materials)
			{
				//material.SetFloat("_Alpha", Mathf.InverseLerp(0f, cameraDistance + alphaOffset, distance));
				material.color = colors_trans[idx++];
			}
		}
		else
		{
			int idx = 0;
			//�@�w�苗����艓���ꍇ�͕W��
			foreach (var material in materials)
			{
				//material.SetFloat("_Alpha", 1f);
				material.color = colors_org[idx++];
			}
		}
	}

	private void OnBecameVisible()
	{
		isVisible = true;
		foreach (var material in materials)
		{
			material.SetFloat("_Alpha", 1f);
		}
	}
	private void OnBecameInvisible()
	{
		isVisible = false;
	}
}