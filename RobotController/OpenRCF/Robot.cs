using System;
using System.Linq;
using System.Reflection;

namespace OpenRCF
{
    public class Robot
    {
        public Kinematics Kinematics;
        public Trajectory Trajectory;
        public Cuboid Torso = new Cuboid(0, 0, 0);        

        public Robot(uint[] DOF)
        {  
            Kinematics = new Kinematics(DOF);            
            Trajectory = new Trajectory(ref Kinematics);
        }

        public Pair this[uint i, uint j]
        {
            get { return Kinematics.Chain[i].Pair[j]; }
        }

        public void Draw()
        {
            Kinematics.Draw();
            Torso.Draw();
        }

        public void SetFloatingJoint6DOF(float sizeX, float sizeY, float sizeZ)
        {
            try
            {
                Kinematics[0, 0].SetPrismatic();
                Kinematics[0, 0].axisInit.SetUnitVectorX();
                Kinematics[0, 0].lInit.SetZeroVector();

                Kinematics[0, 1].SetPrismatic();
                Kinematics[0, 1].axisInit.SetUnitVectorY();
                Kinematics[0, 1].lInit.SetZeroVector();

                Kinematics[0, 2].SetPrismatic();
                Kinematics[0, 2].axisInit.SetUnitVectorZ();
                Kinematics[0, 2].lInit.SetZeroVector();

                Kinematics[0, 3].axisInit.SetUnitVectorX();
                Kinematics[0, 3].lInit.SetZeroVector();
                Kinematics[0, 3].SetJointRangeInfinite();

                Kinematics[0, 4].axisInit.SetUnitVectorY();
                Kinematics[0, 4].lInit.SetZeroVector();
                Kinematics[0, 4].SetJointRangeInfinite();

                Kinematics[0, 5].axisInit.SetUnitVectorZ();
                Kinematics[0, 5].lInit.SetZeroVector();
                Kinematics[0, 5].SetJointRangeInfinite();

                Kinematics[0, 6].lInit.SetZeroVector();

                Torso.Position = Kinematics.Chain[0].pe;
                Torso.Rotate = Kinematics.Chain[0].Re;
                Torso.SetSize(sizeX, sizeY, sizeZ);
            }
            catch
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                Console.WriteLine("DOF must be 6 or more.");
            }
        }

        public void SetPlanarJoint3DOF(float sizeX, float sizeY, float sizeZ)
        {
            try
            {
                Kinematics[0, 0].SetPrismatic();
                Kinematics[0, 0].axisInit.SetUnitVectorX();
                Kinematics[0, 0].lInit.SetZeroVector();

                Kinematics[0, 1].SetPrismatic();
                Kinematics[0, 1].axisInit.SetUnitVectorY();
                Kinematics[0, 1].lInit.SetZeroVector();

                Kinematics[0, 2].axisInit.SetUnitVectorZ();
                Kinematics[0, 2].lInit.SetZeroVector();
                Kinematics[0, 2].SetJointRangeInfinite();

                Kinematics[0, 3].lInit[2] = sizeZ;
                Torso.Position = Kinematics[0, 3].pc;
                Torso.Rotate = Kinematics.Chain[0].Re;
                Torso.SetSize(sizeX, sizeY, sizeZ);
            }
            catch
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                Console.WriteLine("DOF must be 3 or more.");
            }
        }

        public bool IsCollision(params ICollision[] obstacle)
        {
            if (Kinematics.IsCollisionBody(obstacle)) return true;
            if (Kinematics.IsCollisionEffc(obstacle)) return true;
            if (Kinematics.IsCollisionSelf()) return true;
            
            return false;
        }
        
    }

    public class Trajectory
    {
        private Kinematics kinematics;
        private Node[] node = new Node[100];

        public Trajectory(ref Kinematics kinematics)
        {
            this.kinematics = kinematics;

            for (uint i = 0; i < node.Length; i++)
            {
                node[i] = new Node(kinematics.Chain);
            }
        }

        private struct Node
        {
            public uint parent, next;
            public float parentDistance, nextDistance;
            public float[][] q;

            public Node(Chain[] chain)
            {
                parent = 0;
                next = 0;
                parentDistance = 0;
                nextDistance = 0;
                q = new float[chain.Length][];

                for (uint i = 0; i < chain.Length; i++)
                {
                    q[i] = new float[chain[i].DOF];
                }
            }
        }

        public void SetNode(uint nodeNum, uint parentNodeNum)
        {
            if (0 < nodeNum && nodeNum < node.Length && parentNodeNum < node.Length)
            {
                node[nodeNum].parent = parentNodeNum;
                node[parentNodeNum].next = nodeNum;
               
                for (int i = 0; i < kinematics.Chain.Length; i++)
                {
                    for (int j = 0; j < kinematics.Chain[i].DOF; j++)
                    {
                        node[0].q[i][j] = kinematics.Chain[i].Pair[j].qHome;
                        node[nodeNum].q[i][j] = kinematics.Chain[i].Pair[j].q;
                    }
                }

                node[nodeNum].parentDistance = Distance(nodeNum, parentNodeNum);
                node[parentNodeNum].nextDistance = Distance(nodeNum, parentNodeNum);

                PresNodeNum = nodeNum;
            }
            else
            {
                Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                Console.WriteLine("Specify the nodeNum between 1 and " + (node.Length - 1));
                Console.WriteLine("Node 0 is automatically set to the home position.");
            }
        }

        public void GetNode(uint nodeNum)
        {
            PresNodeNum = nodeNum;
    
            for (int i = 0; i < kinematics.Chain.Length; i++)
            {
                for (int j = 0; j < kinematics.Chain[i].DOF; j++)
                {
                    kinematics.Chain[i].Pair[j].q = node[nodeNum].q[i][j];
                }
            }

            kinematics.ForwardKinematics();
        }

        private float speedRatio = 1;
        public float SpeedRatio
        {
            get { return speedRatio; }
            set
            {
                if (0 < value) speedRatio = value;
                else
                {
                    Console.WriteLine("Error : " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                    Console.WriteLine("SpeedRatio must be greater than zero.");
                }
            }
        }

        private uint presNodeNum;
        private float progress = 0;
        private float distanceTop;

        public uint PresNodeNum
        {
            get { return presNodeNum; }
            private set
            {
                distanceTop = 0;
                progress = 0;
                presNodeNum = value;
            }
        }

        private Node PresNode { get { return node[presNodeNum]; } }

        private Node ParentNode { get { return node[node[presNodeNum].parent]; } }

        private Node NextNode { get { return node[node[presNodeNum].next]; } }
        
        private bool isInboundLineProceeded = false;
        private bool isOutboundLineProceeded = false;

        public bool ProceedInboundLine()
        {            
            if (PresNodeNum == 0 || isOutboundLineProceeded)
            {
                isInboundLineProceeded = false;
                return false;
            }
      
            if (0.9999f < progress || PresNode.parentDistance < 0.0001f)
            {
                PresNodeNum = PresNode.parent;
            }
            else
            {
                distanceTop += speedRatio * 0.05f;
                progress = distanceTop / PresNode.parentDistance;
                if (1 < progress) progress = 1;

                for (int i = 0; i < kinematics.Chain.Length; i++)
                {
                    for (int j = 0; j < kinematics.Chain[i].DOF; j++)
                    {
                        kinematics.Chain[i].Pair[j].q = progress * ParentNode.q[i][j] + (1 - progress) * PresNode.q[i][j];
                    }
                }

                kinematics.ForwardKinematics();
            }

            isInboundLineProceeded = true;
            return true;
        }

        public void UpdateOutboundLine(uint goalNodeNum)
        {
            node[goalNodeNum].next = 0;

            uint child = goalNodeNum;
            uint parent;
            uint count = 0;

            while (count < node.Length)
            {
                parent = node[child].parent;
                node[parent].next = child;
                node[parent].nextDistance = Distance(parent, child);

                if (parent == 0) break;
                else child = parent;

                count++;
            }
        }

        public bool ProceedOutboundLine()
        {
            if (PresNode.next == 0 || isInboundLineProceeded)
            {
                isOutboundLineProceeded = false;
                return false;
            }

            if (0.9999f < progress || PresNode.nextDistance < 0.0001f)
            {
                PresNodeNum = PresNode.next;
            }
            else
            {
                distanceTop += speedRatio * 0.05f;
                progress = distanceTop / PresNode.nextDistance;
                if (1 < progress) progress = 1;

                for (int i = 0; i < kinematics.Chain.Length; i++)
                {
                    for (int j = 0; j < kinematics.Chain[i].DOF; j++)
                    {
                        kinematics.Chain[i].Pair[j].q = progress * NextNode.q[i][j] + (1 - progress) * PresNode.q[i][j];
                    }
                }

                kinematics.ForwardKinematics();
            }

            isOutboundLineProceeded = true;
            return true;
        }

        private float Distance(uint nodeNum1, uint nodeNum2)
        {
            float result = 0;

            for (int i = 0; i < node[nodeNum1].q.GetLength(0); i++)
            {
                for (int j = 0; j < node[nodeNum1].q[i].Length; j++)
                {
                    result += (node[nodeNum1].q[i][j] - node[nodeNum2].q[i][j]) * (node[nodeNum1].q[i][j] - node[nodeNum2].q[i][j]);
                }
            }

            return (float)Math.Sqrt(result);
        }

    }

    public class Kinematics
    {        
        public Vector BasePosition = new Vector(3);
        public Chain[] Chain;
        public Target[] Target;

        public ErrorVector e;
        public VirtualSpringMatrix K;        
        public JacobianMatrix J;
        public BlockMatrix JT;
        public BlockVector tau;

        private float Vk, VkOld, delta;
        private BlockVector tauOld, tauTilde, dq;
        private BlockMatrix JTKJ, D, T, E;

        public Kinematics(uint[] DOF)
        {
            Chain = new Chain[DOF.Length];
            Target = new Target[DOF.Length];

            for (uint i = 0; i < DOF.Length; i++)
            {
                Chain[i] = new Chain(DOF[i]);
            }
           
            for (uint i = 0; i < DOF.Length; i++)
            {
                Target[i] = new Target(0.05f);
                Target[i].Position.Set = new float[3] { 1, 0, 1 };
            }

            e = new ErrorVector(DOF);
            K = new VirtualSpringMatrix(DOF);            
            tau = new BlockVector(DOF);
            tauOld = new BlockVector(DOF);
            tauTilde = new BlockVector(DOF);
            dq = new BlockVector(DOF);
            J = new JacobianMatrix(DOF);
            JT = new BlockMatrix(DOF, (uint)DOF.Length, 6);
            JT.FixBlockUpperTriangle();
            JTKJ = new BlockMatrix(DOF, DOF);
            D = new BlockMatrix(DOF, DOF);
            T = new BlockMatrix(DOF, DOF);
            T.FixBlockDiagonal();
            E = new BlockMatrix(DOF, DOF);
            E.FixBlockDiagonal();
        }

        public Pair this[uint i, uint j]
        {
            get { return Chain[i].Pair[j]; }
        }

        public void Draw()
        {
            for (uint i = 0; i < Chain.Length; i++)
            {
                Chain[i].Draw();
                Target[i].Draw();
            }
        }

        public void SetJointLinkRadiusAuto(float ratio = 0.2f)
        {
            float sum = 0;
            int n = 0;

            for (uint i = 0; i < Chain.Length; i++)
            {
                for (uint j = 0; j < Chain[i].DOF + 1; j++)
                {
                    if (0 < Chain[i].Pair[j].lInit.AbsSum)
                    {
                        sum += Chain[i].Pair[j].lInit.Norm;
                        n++;
                    }
                }
            }

            float jointRadius;

            if (0 < n) jointRadius = ratio * sum / n;
            else jointRadius = 0;

            for (uint i = 0; i < Chain.Length; i++)
            {
                for (uint j = 0; j < Chain[i].DOF + 1; j++)
                {
                    Chain[i].Pair[j].Joint.Radius = jointRadius;
                    Chain[i].Pair[j].Link.Radius = jointRadius;
                }

                Target[i].Diameter = jointRadius;
            }
        }

        public void ResetTargets()
        {
            for (uint i = 0; i < Chain.Length; i++)
            {
                Target[i].Position.Set = Chain[i].pe.Get;
                Target[i].Rotate.Set = Chain[i].Re.Get;
            }
        }

        public void ResetHomePosition()
        {
            for (uint i = 0; i < Chain.Length; i++)
            {
                for (uint j = 0; j < Chain[i].DOF; j++)
                {
                    Chain[i].Pair[j].q = Chain[i].Pair[j].qHome;
                }
            }
        }

        public void ForwardKinematics()
        {
            for (int i = 0; i < Chain.Length; i++)
            {
                if (i == 0) Chain[i].Pair[0].R.SetIdentity();
                else Chain[i].Pair[0].R.Set = Chain[0].PairEnd.R.Get;

                for (int j = 0; j < Chain[i].DOF; j++)
                {
                    if (Chain[i].Pair[j].IsRevolute || Chain[i].Pair[j].IsParallelA)
                    {
                        Chain[i].Pair[j + 1].R.Set = Chain[i].Pair[j].R.TimesRn(Chain[i].Pair[j].q, Chain[i].Pair[j].axisInit.Get);
                    }
                    else if (Chain[i].Pair[j].IsPrismatic)
                    {
                        Chain[i].Pair[j + 1].R.Set = Chain[i].Pair[j].R.Get;
                    }
                    else if (Chain[i].Pair[j].IsParallelB)
                    {
                        Chain[i].Pair[j + 1].R.Set = Chain[i].Pair[j - 1].R.Get;
                    }
                }
            }

            for (int i = 0; i < Chain.Length; i++)
            {
                for (int j = 0; j < Chain[i].DOF + 1; j++)
                {
                    Chain[i].Pair[j].axis.Set = Chain[i].Pair[j].R.Times(Chain[i].Pair[j].axisInit.Get);
                    Chain[i].Pair[j].l.Set = Chain[i].Pair[j].R.Times(Chain[i].Pair[j].lInit.Get);
                }
            }

            for (int i = 0; i < Chain.Length; i++)
            {
                if (i == 0) Chain[i].Pair[0].p.Set = Chain[i].Pair[0].l.Plus(BasePosition.Get);
                else Chain[i].Pair[0].p.Set = Chain[i].Pair[0].l.Plus(Chain[0].PairEnd.p.Get);

                for (int j = 0; j < Chain[i].DOF; j++)
                {
                    Chain[i].Pair[j + 1].p.Set = Chain[i].Pair[j].p.Plus(Chain[i].Pair[j + 1].l.Get);

                    if (Chain[i].Pair[j].IsPrismatic)
                    {
                        Chain[i].Pair[j + 1].p.SetPlus(Chain[i].Pair[j].axis.Times(Chain[i].Pair[j].q));
                    }
                }
            }

            for (int i = 0; i < Chain.Length; i++)
            {
                for (int j = 0; j < Chain[i].DOF + 1; j++)
                {
                    Chain[i].Pair[j].pt.Set = Chain[i].Pair[j].p.Plus(Chain[i].Pair[j].l.Get, 1, -0.25f);
                    Chain[i].Pair[j].pc.Set = Chain[i].Pair[j].p.Plus(Chain[i].Pair[j].l.Get, 1, -0.50f);
                    Chain[i].Pair[j].pb.Set = Chain[i].Pair[j].p.Plus(Chain[i].Pair[j].l.Get, 1, -0.75f);
                }

                Chain[i].pe.Set = Chain[i].PairEnd.p.Get;
                Chain[i].Re.Set = Chain[i].PairEnd.R.Times(Chain[i].PairEnd.lInit.ConvertRotationMatrix);
            }
        }

        public void InverseKinematics()
        {
            bool isReseted = false;

        LabelStart:

            K.ZetaReset(Chain, Target);
    
            tauTilde.SetZeroVector();
            tau.SetZeroVector();

            uint k = 0;
            bool isCompleted = false;
            float epsilon = 0.00001f;

            while (k < 100)
            {
                ForwardKinematics();
                e.Update(Chain, Target);

                VkOld = Vk;
                Vk = K.QuadraticForm(e.Get);

                if (k == 0) VkOld = 2 * Vk;
                else if (98 < k) Vk = 0;

                if ((isCompleted && Math.Abs(VkOld - Vk) < epsilon) || Vk < epsilon)
                {
                    if (LimitedJointNum == 0 || isReseted) break;
                    else
                    {
                        ResetLockedJoint();
                        isReseted = true;
                        goto LabelStart;
                    }
                }
                if (VkOld <= Vk || Math.Abs(VkOld - Vk) < epsilon)
                {
                    if (K.IsZetaConverged(Target)) isCompleted = true;
                    else K.ZetaUpdate(Chain, Target);
                }

                J.Update(Chain);
                tauOld.Set = tau.Get;
                tau.Set = J.TransposeTimes(K.Times(e.Get));
                J.ReflectJointMobility(Chain, tau);
                JT.Set = J.Transpose;
                JTKJ.Set = JT.Times(K.Times(J.Get));
                delta = JTKJ.Trace / (1000 * JTKJ.GetLength(0));
                E.SetDiagonal(delta);
                T.SetDiagonal(tauTilde.Plus(tau.Abs));
                CalcTauTilde();
                D.Set = JTKJ.Plus(T.Plus(E.Get));
                D.SetInverse();
                dq.Set = D.Times(tau.Get);
                q = dq.Plus(q);

                k++;
            }

            // Console.WriteLine(k);
        }

        private Vector[] q
        {
            get
            {
                Vector[] result = new Vector[Chain.Length];

                for (uint i = 0; i < Chain.Length; i++)
                {
                    result[i] = new Vector(Chain[i].DOF);

                    for (uint j = 0; j < Chain[i].DOF; j++)
                    {
                        result[i][j] = Chain[i].Pair[j].q;
                    }
                }

                return result;
            }
            set
            {
                for (uint i = 0; i < Chain.Length; i++)
                {
                    for (uint j = 0; j < Chain[i].DOF; j++)
                    {
                        Chain[i].Pair[j].q = value[i][j];
                    }
                }
            }
        }

        private void ResetLockedJoint()
        {         
            for (uint i = 0; i < Chain.Length; i++)
            {
                for (uint j = 0; j < Chain[i].DOF; j++)
                {
                    if (Chain[i].Pair[j].IsLimited && !Chain[i].Pair[j].IsPrismatic)
                    {
                        Chain[i].Pair[j].q = Chain[i].Pair[j].qHome;
                    }
                }
            }
        }

        private uint LimitedJointNum
        {
            get
            {
                uint result = 0;

                for (uint i = 0; i < Chain.Length; i++)
                {
                    for (uint j = 0; j < Chain[i].DOF; j++)
                    {
                        if (Chain[i].Pair[j].IsRevolute || Chain[i].Pair[j].IsParallelA)
                        {
                            if (Chain[i].Pair[j].IsLimited) result++;
                        }
                    }
                }

                return result;
            }
        }

        private void CalcTauTilde()
        {
            for (uint i = 0; i < tau.Length; i++)
            {
                for (uint j = 0; j < tau[i].Length; j++)
                {
                    if (0 <= tauOld[i][j] * tau[i][j])
                    {
                        tauTilde[i][j] -= Math.Abs(tau[i][j]);
                        if (tauTilde[i][j] < 0) tauTilde[i][j] = 0;
                    }
                    else
                    {
                        tauTilde[i][j] += Math.Abs(tau[i][j]);
                    }
                }
            }
        }
              
        public bool IsCollisionBody(params ICollision[] obstacle)
        {
            for (int c = 0; c < obstacle.Length; c++)
            {
                for (int i = 0; i < Chain.Length; i++)
                {
                    for (int j = 0; j < Chain[i].DOF; j++)
                    {
                        if (Chain[i].Pair[j].IsCollision(obstacle[c])) return true;           
                    }
                }
            }

            return false;
        }

        public bool IsCollisionEffc(params ICollision[] obstacle)
        {
            for (int c = 0; c < obstacle.Length; c++)
            {
                for (int i = 0; i < Chain.Length; i++)
                {
                    if (Chain[i].PairEnd.IsCollision(obstacle[c])) return true;                           
                }
            }

            return false;
        }

        public bool IsCollisionSelf()
        {
            for (int I = 0; I < Chain.Length; I++)
            {
                for (int i = 0; i < Chain.Length; i++)
                {
                    for (int j = 0; j < Chain[i].DOF + 1; j++)
                    {
                        if (I == i && Chain[i].DOF - 1 <= j) break;
                        else if (I == 0 && 0 < i && j == 0) break;

                        if (Chain[i].Pair[j].IsCollision(Chain[I].PairEnd.p.Get, 0.5f * Chain[I].PairEnd.Joint.Radius)) return true;
                        if (Chain[i].Pair[j].IsCollision(Chain[I].PairEnd.pc.Get, 0.5f * Chain[I].PairEnd.Joint.Radius)) return true;
                    }                   
                }
            }

            return false;
        }

    }

    public class ErrorVector : BlockVector
    {
        public ErrorVector(uint[] DOF) : base((uint)DOF.Length, 6) { }
        
        private RotationMatrix dRRT = new RotationMatrix();
        private Vector dn = new Vector(3);
        private Vector no = new Vector(3);
        public void Update(Chain[] chain, Target[] target)
        {
            for (uint i = 0; i < chain.Length; i++)
            {
                if (target[i].DOF == 6)
                {
                    dRRT.Set = target[i].Rotate.Times(chain[i].Re.Transpose);
                    this[i].SetStack(target[i].Position.Minus(chain[i].pe.Get), dRRT.AngleAxisVector);
                }
                else if (target[i].DOF == 5)
                {
                    dn.Set = target[i].Rotate.GetColumn(2);
                    no.Set = dn.NormalizedCrossProduct(chain[i].Re.GetColumn(2));
                    this[i].SetStack(target[i].Position.Minus(chain[i].pe.Get), no.Times(-dn.FormedAngle(chain[i].Re.GetColumn(2))));
                }
                else if (target[i].DOF == 3)
                {
                    this[i].SetStack(target[i].Position.Minus(chain[i].pe.Get), new float[3]);
                }
                else
                {
                    this[i].SetZeroVector();
                }
            }
        }
    }

    public class VirtualSpringMatrix : BlockMatrix
    {
        private float[] zeta;
       
        public VirtualSpringMatrix(uint[] DOF) : base((uint)DOF.Length, 6, (uint)DOF.Length, 6)
        {
            zeta = Enumerable.Repeat<float>(1, DOF.Length).ToArray();
            FixBlockDiagonal();
        }
       
        public void Update(Chain[] chain, Target[] target)
        {
            float lengthMax = chain[0].LinkLength;

            for (int i = 1; i < chain.Length; i++)
            {
                if (lengthMax < chain[i].LinkLength) lengthMax = chain[i].LinkLength;
            }

            float Kf, Km;
            for (uint i = 0; i < chain.Length; i++)
            {
                if (0 < target[i].DOF) Kf = zeta[i] * 1;
                else Kf = 0;

                if (3 < target[i].DOF) Km = zeta[i] * lengthMax * lengthMax / (float)Math.PI;
                else Km = 0;

                this[i, i].SetDiagonal(new float[6] { Kf, Kf, Kf, Km, Km, Km });
            }
        }

        public void ZetaUpdate(Chain[] chain, Target[] target)
        {
            for (uint i = 0; i < target.Length; i++)
            {
                if (target[i].Priority == false && 0.249f < zeta[i])
                {
                    zeta[i] -= 0.25f;
                }
            }

            Update(chain, target);
        }

        public bool IsZetaConverged(Target[] target)
        {
            for (uint i = 0; i < target.Length; i++)
            {
                if (0 < target[i].DOF && !target[i].Priority && 0.01f < zeta[i]) return false;
            }

            return true;
        }

        public void ZetaReset(Chain[] chain, Target[] target)
        {
            for (uint i = 0; i < zeta.Length; i++)
            {
                zeta[i] = 1;
            }

            Update(chain, target);
        }

    }

    public class JacobianMatrix : BlockMatrix
    {
        public JacobianMatrix(uint[] DOF) : base((uint)DOF.Length, 6, DOF)
        {
            FixBlockLowerTriangle();
        }

        private Vector sj = new Vector(6);
        private Vector bj = new Vector(6);
        public void Update(Chain[] chain)
        {
            for (uint i = 0; i < chain.Length; i++)
            {
                for (uint j = 0; j < chain[0].DOF; j++)
                {
                    if (chain[0].Pair[j].IsLocked)
                    {
                        sj.SetZeroVector();
                    }
                    else
                    {
                        if (chain[0].Pair[j].IsRevolute)
                        {
                            sj.SetStack(chain[0].Pair[j].axis.TimesCross(chain[i].pe.Minus(chain[0].Pair[j].p.Get)), chain[0].Pair[j].axis.Get);
                        }
                        else if (chain[0].Pair[j].IsPrismatic)
                        {
                            sj.SetStack(chain[0].Pair[j].axis.Get, new float[3]);
                        }
                        else if (chain[0].Pair[j].IsParallelA)
                        {
                            sj.SetStack(chain[0].Pair[j].axis.TimesCross(chain[0].Pair[j + 1].l.Get), new float[3]);
                        }
                        else
                        {
                            sj.SetZeroVector();
                        }
                    }

                    this[i, 0].SetColumn(j, sj.Get);
                }
            }

            // Diagnal Block 
            for (uint i = 1; i < chain.Length; i++)
            {
                for (uint j = 0; j < chain[i].DOF; j++)
                {
                    if (chain[i].Pair[j].IsLocked)
                    {
                        bj.SetZeroVector();
                    }
                    else
                    {
                        if (chain[i].Pair[j].IsRevolute)
                        {
                            bj.SetStack(chain[i].Pair[j].axis.TimesCross(chain[i].pe.Minus(chain[i].Pair[j].p.Get)), chain[i].Pair[j].axis.Get);
                        }
                        else if (chain[i].Pair[j].IsPrismatic)
                        {
                            bj.SetStack(chain[i].Pair[j].axis.Get, new float[3]);
                        }
                        else if (chain[i].Pair[j].IsParallelA)
                        {
                            bj.SetStack(chain[i].Pair[j].axis.TimesCross(chain[i].Pair[j + 1].l.Get), new float[3]);
                        }
                        else
                        {
                            bj.SetZeroVector();
                        }
                    }

                    this[i, i].SetColumn(j, bj.Get);
                }
            }
        }

        public void ReflectJointMobility(Chain[] chain, BlockVector tau)
        {
            for (uint i = 0; i < chain.Length; i++)
            {
                for (uint j = 0; j < chain[0].DOF; j++)
                {
                    if (chain[0].Pair[j].IsDrivable(tau[0][j]) == false)
                    {
                        this[i, 0].SetColumn(j, new float[6]);
                    }
                }
            }

            for (uint i = 1; i < chain.Length; i++)
            {
                for (uint j = 0; j < chain[i].DOF; j++)
                {
                    if (chain[i].Pair[j].IsDrivable(tau[i][j]) == false)
                    {
                        this[i, i].SetColumn(j, new float[6]);
                    }
                }
            }
        }

    }

    public class Target : PrickleBall
    {        
        public bool Priority = true;
     
        public uint DOF { get; private set; } = 6;
        public void SetDOF6() { DOF = 6; }
        public void SetDOF5() { DOF = 5; }
        public void SetDOF3() { DOF = 3; }        
        public void SetDOF0() { DOF = 0; }

        public Target(float diameter)
        {
            Diameter = diameter;
            Color.SetDarkGray();
            Rotate.SetRy(0.5 * Math.PI);
        }

        new public void Draw()
        {
            if (0 < DOF)
            {
                if (DOF == 6) base.ConeNum = 3;
                else if (DOF == 5) base.ConeNum = 1;
                else if (DOF == 3) base.ConeNum = 0;
                base.Draw();
            }
        }
    }

    public class Chain
    {
        public Pair[] Pair;
        public PrickleBall EndPoint = new PrickleBall();

        public Vector pe = new Vector(3);
        public RotationMatrix Re = new RotationMatrix();

        public Chain(uint DOF)
        {
            this.DOF = DOF;
            
            EndPoint.Position = pe;
            EndPoint.Rotate = Re;
        }

        public uint DOF
        {
            get { return (uint)(Pair.Length - 1); }
            set
            {
                Pair = new Pair[value + 1];

                for (uint i = 0; i < Pair.Length; i++)
                {
                    Pair[i] = new Pair();
                }
            }
        }
       
        public Pair PairEnd { get { return Pair[DOF]; } }
    
        public float LinkLength
        {
            get
            {
                float result = 0;

                for (int i = 0; i < Pair.Length; i++)
                {
                    result += Pair[i].lInit.Norm;
                }

                return result;
            }
        }

        public void Draw()
        {
            Pair[0].DrawBaseLink();
     
            for (int i = 0; i < DOF; i++)
            {
                Pair[i].DrawLink();
                Pair[i].DrawJoint();
            }

            Pair[DOF].DrawEndLink();

            EndPoint.Diameter = Pair[DOF].Joint.Radius;
            EndPoint.Draw();
        }

    }

    public class Pair
    {        
        public Vector p = new Vector(3);
        public RotationMatrix R = new RotationMatrix();
        public Vector l = new Vector(3);
        public Vector lInit = new Vector(3);
        public Vector axis = new Vector(3);
        public Vector axisInit = new Vector(3);
   
        public Vector pt = new Vector(3);
        public Vector pc = new Vector(3);
        public Vector pb = new Vector(3);

        public RobotObject.Joint Joint = new RobotObject.Joint();
        public RobotObject.Link Link = new RobotObject.Link();
       
        public Pair(float jointRadius = 0.05f)
        {
            lInit[2] = 0.2f;
            axisInit.SetUnitVectorY();
            
            Joint.Radius = jointRadius;        
            Joint.Position = p;
            Joint.Rotate = R;
            Joint.axisInit = axisInit;

            Link.Radius = jointRadius;
            Link.Position = pc;
            Link.Rotate = R;
            Link.lInit = lInit;
        }

        public void DrawLink()
        {  
            Link.Draw(Joint.Radius);
        }

        public void DrawBaseLink()
        {  
            Link.DrawBase(Joint.Radius);
        }

        public void DrawEndLink()
        {    
            Link.DrawEnd(Joint.Radius);
        }

        public void DrawJoint()
        {     
            if (isLimited) Joint.EnableLimitedColor();
            else Joint.DisableLimitedColor();

            if (IsRevolute || IsParallelA || IsParallelB) Joint.DrawRevolute();
            else if (IsPrismatic) Joint.DrawPrismatic(q);
        }

        private float[][] CheckPoint
        {
            get
            {
                if (0.001f < Link.Radius)
                {
                    int num = (int)(lInit.Norm / (2 * Link.Radius)) + 1;

                    float[][] result = new float[num][];
                    float t = 1f / (num + 1);

                    for (int i = 0; i < num; i++)
                    {
                        result[i] = p.Plus(l.Get, 1, -t);
                        t += 1f / (num + 1);
                    }

                    return result;
                }
                else
                {
                    return new float[0][];
                }
            }
        }

        private float Distance(float[] p1, float[] p2)
        {
            float[] e = new float[3] { p1[0] - p2[0], p1[1] - p2[1], p1[2] - p2[2] };
            return (float)Math.Sqrt(e[0] * e[0] + e[1] * e[1] + e[2] * e[2]);
        }

        public bool IsCollision(float[] position, float threshold = 0)
        {
            if (0.001f < lInit.AbsSum)
            {                
                float d = p.Distance(position);

                if (threshold + Joint.Radius + lInit.AbsSum < d) return false;
                else if (d < threshold + Joint.Radius) return true;
                else
                {
                    float[][] checkPoint = CheckPoint;

                    for (int i = 0; i < checkPoint.GetLength(0); i++)
                    {
                        if (Distance(checkPoint[i], position) < threshold + Link.Radius) return true;
                    }
                }
            }

            return false;
        }

        public bool IsCollision(ICollision obstacle, float threshold = 0)
        {
            if (0.001f < lInit.AbsSum)
            {
                if (!obstacle.IsCollision(p.Get, threshold + Joint.Radius + lInit.AbsSum)) return false;
                else if (obstacle.IsCollision(p.Get, threshold + Joint.Radius)) return true;
                else
                {
                    float[][] checkPoint = CheckPoint;

                    for (int i = 0; i < checkPoint.GetLength(0); i++)
                    {
                        if (obstacle.IsCollision(checkPoint[i], threshold + Link.Radius)) return true;
                    }
                }
            }

            return false;
        }

        public float[] JointRange = new float[2] { -0.75f * (float)Math.PI, 0.75f * (float)Math.PI };

        public float JointRangeMin {
            get { return JointRange[0]; }
            set { JointRange[0] = value; } 
        }

        public float JointRangeMax {
            get { return JointRange[1]; }
            set { JointRange[1] = value; } 
        }

        public void SetJointRangeInfinite()
        {
            JointRange[0] = float.MinValue;
            JointRange[1] = float.MaxValue;
        }

        private bool isLimited = false;
        private float _q;

        public float q
        {
            get{ return _q;}
            set
            {
                _q = value;

                if (float.IsNaN(_q))
                {
                    Console.WriteLine("Error : NaN is assigned to q");
                    _q = 0;
                }

                if (_q <= JointRange[0])
                {
                    _q = JointRange[0];
                    isLimited = true;
                }
                else if (JointRange[1] <= _q)
                {
                    _q = JointRange[1];
                    isLimited = true;
                }
                else
                {
                    isLimited = false;
                }
            }
        }

        public float _qHome;

        public float qHome
        {
            get { return _qHome; }
            set
            {
                q = value;
                _qHome = value;                
            }
        }

        public bool IsLimited { get { return isLimited; } }
    
        public bool IsDrivable(float direction)
        {
            if (isLimited)
            {
                if (q <= 0.5f * (JointRange[0] + JointRange[1]) && direction <= 0) return false;
                else if (0.5f * (JointRange[0] + JointRange[1]) <= q && 0 <= direction) return false;
            }

            return true;
        }


        private short jointType = 0;
      
        public void SetRevolute() { jointType = 0; }

        public void SetPrismatic() { jointType = 1; }

        public void SetParallelA() { jointType = 2; }

        public void SetParallelB() { jointType = 3; }


        private bool isJointLocked = false;

        public void EnableJointLock() { isJointLocked = true; }

        public void DisableJointLock() { isJointLocked = false; }


        public bool IsRevolute
        {
            get
            {
                if (jointType == 0) return true;
                else return false;
            }
        }

        public bool IsPrismatic
        {
            get
            {
                if (jointType == 1) return true;
                else return false;
            }
        }

        public bool IsParallelA
        {
            get
            {
                if (jointType == 2) return true;
                else return false;
            }
        }

        public bool IsParallelB
        {
            get
            {
                if (jointType == 3) return true;
                else return false;
            }
        }

        public bool IsLocked
        {
            get { return isJointLocked; }
        }


    }

}
