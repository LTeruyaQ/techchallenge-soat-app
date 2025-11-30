# ============================================
# Kubernetes Namespace
# ============================================

resource "kubectl_manifest" "namespace" {
  depends_on = [
    aws_eks_cluster.eks,
    aws_eks_node_group.nodes,
    aws_eks_access_entry.lab_role,
    aws_eks_access_policy_association.lab_role_admin
  ]

  yaml_body = <<YAML
apiVersion: v1
kind: Namespace
metadata:
  name: mecanicaos
YAML
}
