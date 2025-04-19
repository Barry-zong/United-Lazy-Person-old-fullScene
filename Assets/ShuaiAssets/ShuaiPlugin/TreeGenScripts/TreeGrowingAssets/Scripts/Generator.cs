using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 添加树叶生长控制器引用
using TreeGrowingAssets.Scripts;

/**
 * @author Ciphered <https://ciphered.xyz
 * 
 * Generates a tree
 * Article link
 **/
public class Generator : MonoBehaviour {

	/**
	 * Represents a branch 
	 **/
	public class Branch {
		public Vector3 _start;
		public Vector3 _end;
		public Vector3 _direction;
		public Branch _parent;
		public float _size;
		public float _lastSize;
		public List<Branch> _children = new List<Branch>();
		public List<Vector3> _attractors = new List<Vector3>();
		public int _verticesId; // the index of the vertices within the vertices array 
		public int _distanceFromRoot = 0;
		public bool _grown = false;

		public Branch(Vector3 start, Vector3 end, Vector3 direction, Branch parent = null) {
			_start = start;
			_end = end;
			_direction = direction;
			_parent = parent;
		}
	}


	[Header("Generation parameters")]
	[Range(0, 3000)]
	public int _nbAttractors = 400;
	[Range(0f, 10f)]
	public float _radius = 5f;
	public Vector3 _startPosition = new Vector3(0, 0, 0);
	[Range(0f, 0.5f)]
	public float _branchLength = 0.2f;
	[Range(0f, 1f)]
	public float _timeBetweenIterations = 0.5f;
	[Range(0f, 3f)]
	public float _attractionRange = 0.1f;
	[Range(0f, 2f)]
	public float _killRange = 0.5f;
	[Range(0f, 0.2f)]
	public float _randomGrowth = 0.1f;
	[Tooltip("是否使用随机种子")]
	public bool _useRandomSeed = false;
	[Tooltip("随机种子，相同的种子会产生相同的树形")]
	public int _randomSeed = 0;

	[Header("生长控制")]
	[Tooltip("是否暂停生长")]
	public bool _pauseGrowth = false;

	[Header("树叶设置")]
	[Tooltip("树叶预制体数组，最多4个")]
	public GameObject[] _leafPrefabs = new GameObject[4]; // 树叶预制体数组
	[Range(0f, 1f)]
	public float _leafSpawnProbability = 0.3f; // 生成树叶的概率
	[Range(0f, 2f)]
	public float _leafSize = 0.1f; // 树叶基础大小
	[Range(0f, 55f)]
	public float _leafRotationRandomness = 0.2f; // 树叶旋转随机度
	[Range(0, 10)]
	public int _minDistanceFromRootForLeaves = 3; // 距离根节点最小距离才开始生成树叶

	[Header("Mesh generation")]
	[Range(0, 20)]
	public int _radialSubdivisions = 10;
	[Range(0f, 1f), Tooltip("The size at the extremity of the branches")]
	public float _extremitiesSize = 0.05f;
	[Range(0f, 5f), Tooltip("Growth power, of the branches size")]
	public float _invertGrowth = 2f;


	// the attractor points
	public List<Vector3> _attractors = new List<Vector3>();

	// a list of the active attractors 
	public List<int> _activeAttractors = new List<int>();

	// reference to the first branch 
	Branch _firstBranch;

	// the branches 
	List<Branch> _branches = new List<Branch>();

	// a list of the current extremities 
	public List<Branch> _extremities = new List<Branch>();

	// the elpsed time since the last iteration, this is used for the purpose of animation
	float _timeSinceLastIteration = 0f;

	MeshFilter _filter;


	void Awake () {
		// initilization 
	}

	/**
	 * Generates n attractors and stores them in the attractors array
	 * The points are generated within a sphere of radius r using a random distribution
	 **/
	void GenerateAttractors (int n, float r) {
		for (int i = 0; i < n; i++) {
			float radius = Random.Range(0f, 1f);
			radius = Mathf.Pow(Mathf.Sin(radius * Mathf.PI/2f), 0.8f);
			radius*= r;
			// 2 angles are generated from which a direction will be computed
			float alpha = Random.Range(0f, Mathf.PI);
			float theta = Random.Range(0f, Mathf.PI*2f);

			Vector3 pt = new Vector3(
				radius * Mathf.Cos(theta) * Mathf.Sin(alpha),
				radius * Mathf.Sin(theta) * Mathf.Sin(alpha),
				radius * Mathf.Cos(alpha)
			);

			// translation to match the parent position
			pt+= transform.position;

			_attractors.Add(pt);
		}
	}

	/**
	 * Returns a 3D random vector of _randomGrowth magniture 
	 **/
	Vector3 RandomGrowthVector () {
		// 使用分支的ID和距离根节点的距离来生成固定的随机值
		float alpha = Mathf.PerlinNoise(_timeSinceLastIteration * 0.1f, _branches.Count * 0.1f) * Mathf.PI;
		float theta = Mathf.PerlinNoise(_branches.Count * 0.1f, _timeSinceLastIteration * 0.1f) * Mathf.PI * 2f;

		Vector3 pt = new Vector3(
			Mathf.Cos(theta) * Mathf.Sin(alpha),
			Mathf.Sin(theta) * Mathf.Sin(alpha),
			Mathf.Cos(alpha)
		);

		return pt * _randomGrowth;
	}

	// Start is called before the first frame update
	void Start () {
		// 根据设置决定是否使用随机种子
		if (_useRandomSeed) {
			Random.InitState(_randomSeed);
		} else {
			Random.InitState(System.Environment.TickCount);
		}
		
		GenerateAttractors(_nbAttractors, _radius);

		_filter = GetComponent<MeshFilter>();

		// we generate the first branch 
		_firstBranch = new Branch(_startPosition, _startPosition + new Vector3(0, _branchLength, 0), new Vector3(0, 1, 0));
		_branches.Add(_firstBranch);
		_extremities.Add(_firstBranch);
	}

  // Update is called once per frame
  void Update () {
		// 自动生长模式
		if (!_pauseGrowth) {
			_timeSinceLastIteration += Time.deltaTime;
			if (_timeSinceLastIteration > _timeBetweenIterations) {
				_timeSinceLastIteration = 0f;
				GrowOneStep();
			}
		}
	}

	/**
	 * 执行一次生长步骤
	 **/
	void GrowOneStep() {
		// 标记已生长的末端
		foreach (Branch b in _extremities) {
			b._grown = true;
		}

		// 移除近距离吸引点
		for (int i = _attractors.Count-1; i >= 0; i--) {
			foreach (Branch b in _branches) {
				if (Vector3.Distance(b._end, _attractors[i]) < _killRange) {
					_attractors.Remove(_attractors[i]);
					_nbAttractors--;
					break;
				}
			}
		}

		if (_attractors.Count > 0) {
			// 分配吸引点到分支
			AssignAttractorsToBranches();
			
			// 生成新分支
			if (_activeAttractors.Count != 0) {
				GenerateNewBranches();
			} else {
				GrowExtremities();
			}
		}

		ToMesh();
	}

	/**
	 * 重置树
	 **/
	public void ResetTree() {
		// 清除现有数据
		_attractors.Clear();
		_activeAttractors.Clear();
		_branches.Clear();
		_extremities.Clear();
		_timeSinceLastIteration = 0f;

		// 重新生成吸引点
		GenerateAttractors(_nbAttractors, _radius);

		// 重新生成第一个分支
		_firstBranch = new Branch(_startPosition, _startPosition + new Vector3(0, _branchLength, 0), new Vector3(0, 1, 0));
		_branches.Add(_firstBranch);
		_extremities.Add(_firstBranch);

		// 更新网格
		ToMesh();
	}

	/**
	 * Creates a mesh from the branches list
	 **/
	void ToMesh () {
		Mesh treeMesh = new Mesh();

		// we first compute the size of each branch 
		for (int i = _branches.Count-1; i >= 0; i--) {
			float size = 0f;
			Branch b = _branches[i];
			if (b._children.Count == 0) {
				size = _extremitiesSize;
			} else {
				foreach (Branch bc in b._children) {
					size+= Mathf.Pow(bc._size, _invertGrowth);
				}
				size = Mathf.Pow(size, 1f/_invertGrowth);
			}
			b._size = size;
		}

		Vector3[] vertices = new Vector3[(_branches.Count+1) * _radialSubdivisions];
		int[] triangles = new int[_branches.Count * _radialSubdivisions * 6];

		// construction of the vertices 
		for (int i = 0; i < _branches.Count; i++) {
			Branch b = _branches[i];

			// the index position of the vertices
			int vid = _radialSubdivisions*i;
			b._verticesId = vid;

			// quaternion to rotate the vertices along the branch direction
			Quaternion quat = Quaternion.FromToRotation(Vector3.up, b._direction);

			// construction of the vertices 
			for (int s = 0; s < _radialSubdivisions; s++) {
				// radial angle of the vertex
				float alpha = ((float)s/_radialSubdivisions) * Mathf.PI * 2f;

				// radius is hard-coded to 0.1f for now
				Vector3 pos = new Vector3(Mathf.Cos(alpha)* b._size, 0, Mathf.Sin(alpha) * b._size);
				pos = quat * pos; // rotation

				// if the branch is an extremity, we have it growing slowly
				if (b._children.Count == 0 && !b._grown) {
					pos+= b._start + (b._end-b._start) * _timeSinceLastIteration/_timeBetweenIterations;
				} else {
					pos+= b._end;
				}

				vertices[vid+s] = pos - transform.position; // from tree object coordinates to [0; 0; 0]

				// if this is the tree root, vertices of the base are added at the end of the array 
				if (b._parent == null) {
					vertices[_branches.Count*_radialSubdivisions+s] = b._start + new Vector3(Mathf.Cos(alpha)* b._size, 0, Mathf.Sin(alpha)*b._size) - transform.position;
				}
			}
		}

		// faces construction; this is done in another loop because we need the parent vertices to be computed
		for (int i = 0; i < _branches.Count; i++) {
			Branch b = _branches[i];
			int fid = i*_radialSubdivisions*2*3;
			// index of the bottom vertices 
			int bId = b._parent != null ? b._parent._verticesId : _branches.Count*_radialSubdivisions;
			// index of the top vertices 
			int tId = b._verticesId;

			// construction of the faces triangles
			for (int s = 0; s < _radialSubdivisions; s++) {
				// the triangles 
				triangles[fid+s*6] = bId+s;
				triangles[fid+s*6+1] = tId+s;
				if (s == _radialSubdivisions-1) {
					triangles[fid+s*6+2] = tId;
				} else {
					triangles[fid+s*6+2] = tId+s+1;
				}

				if (s == _radialSubdivisions-1) {
					// if last subdivision
					triangles[fid+s*6+3] = bId+s;
					triangles[fid+s*6+4] = tId;
					triangles[fid+s*6+5] = bId;
				} else {
					triangles[fid+s*6+3] = bId+s;
					triangles[fid+s*6+4] = tId+s+1;
					triangles[fid+s*6+5] = bId+s+1;
				}
			}
		}

		treeMesh.vertices = vertices;
		treeMesh.triangles = triangles;
		treeMesh.RecalculateNormals();
		_filter.mesh = treeMesh;
	}

	void OnDrawGizmos () {
		/*
		if (_attractors == null) {
			return;
		}
		// we draw the attractors
		for (int i = 0; i < _attractors.Count; i++) {
			if (_activeAttractors.Contains(i)) {
				Gizmos.color = Color.yellow;
			} else {
				Gizmos.color = Color.red;
			}
			Gizmos.DrawSphere(_attractors[i], 0.22f);
		}

		Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 0.4f);
		Gizmos.DrawSphere(_extremities[0]._end, _attractionRange);
		*/

		// we draw the branches 
		foreach (Branch b in _branches) {
			Gizmos.color = Color.green;
			Gizmos.DrawLine(b._start, b._end);
			Gizmos.color = Color.magenta;
			Gizmos.DrawSphere(b._end, 0.05f);
			Gizmos.DrawSphere(b._start, 0.05f);
		}
	}

	/**
	 * 在分支上生成树叶
	 **/
	void SpawnLeaf(Branch branch) {
		if (_leafPrefabs == null || _leafPrefabs.Length == 0) return;
		if (branch._distanceFromRoot < _minDistanceFromRootForLeaves) return;
		
		// 使用Perlin噪声生成固定的随机值
		float randomValue = Mathf.PerlinNoise(branch._distanceFromRoot * 0.1f, _branches.Count * 0.1f);
		if (randomValue > _leafSpawnProbability) return;

		// 计算树叶位置（在分支的中间位置）
		Vector3 leafPosition = Vector3.Lerp(branch._start, branch._end, 0.5f);
		
		// 计算树叶旋转
		Quaternion leafRotation = Quaternion.LookRotation(branch._direction);
		// 添加随机旋转
		float randomRotation = Mathf.PerlinNoise(branch._distanceFromRoot * 0.2f, _branches.Count * 0.2f) * _leafRotationRandomness;
		leafRotation *= Quaternion.Euler(
			randomRotation * 360f,
			randomRotation * 360f,
			randomRotation * 360f
		);

		// 随机选择一个树叶预制体
		int randomIndex = Random.Range(0, _leafPrefabs.Length);
		if (_leafPrefabs[randomIndex] == null) return; // 确保选中的预制体不为空

		// 实例化树叶
		GameObject leaf = Instantiate(_leafPrefabs[randomIndex], leafPosition, leafRotation, transform);
		
		// 添加生长控制器并初始化
		LeafGrowthController growthController = leaf.AddComponent<LeafGrowthController>();
		growthController.Initialize(Vector3.one * _leafSize);
	}

	/**
	 * 分配吸引点到分支
	 **/
	void AssignAttractorsToBranches() {
		_activeAttractors.Clear();
		foreach (Branch b in _branches) {
			b._attractors.Clear();
		}

		int ia = 0;
		foreach (Vector3 attractor in _attractors) {
			float min = 999999f;
			Branch closest = null;
			foreach (Branch b in _branches) {
				float d = Vector3.Distance(b._end, attractor);
				if (d < _attractionRange && d < min) {
					min = d;
					closest = b;
				}
			}

			if (closest != null) {
				closest._attractors.Add(attractor);
				_activeAttractors.Add(ia);
			}

			ia++;
		}
	}

	/**
	 * 生成新分支
	 **/
	void GenerateNewBranches() {
		_extremities.Clear();
		List<Branch> newBranches = new List<Branch>();

		foreach (Branch b in _branches) {
			if (b._attractors.Count > 0) {
				Vector3 dir = new Vector3(0, 0, 0);
				foreach (Vector3 attr in b._attractors) {
					dir += (attr - b._end).normalized;
				}
				dir /= b._attractors.Count;
				dir += RandomGrowthVector();
				dir.Normalize();

				Branch nb = new Branch(b._end, b._end + dir * _branchLength, dir, b);
				nb._distanceFromRoot = b._distanceFromRoot + 1;
				b._children.Add(nb);
				newBranches.Add(nb);
				_extremities.Add(nb);

				SpawnLeaf(nb);
			} else {
				if (b._children.Count == 0) {
					_extremities.Add(b);
				}
			}
		}

		_branches.AddRange(newBranches);
	}

	/**
	 * 生长末端分支
	 **/
	void GrowExtremities() {
		for (int i = 0; i < _extremities.Count; i++) {
			Branch e = _extremities[i];
			Vector3 start = e._end;
			Vector3 dir = e._direction + RandomGrowthVector();
			Vector3 end = e._end + dir * _branchLength;
			Branch nb = new Branch(start, end, dir, e);

			e._children.Add(nb);
			_branches.Add(nb);
			_extremities[i] = nb;
		}
	}
}
