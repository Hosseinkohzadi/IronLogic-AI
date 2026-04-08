export type UserStatus = 'Active' | 'Review' | 'Suspended';

export interface UserRow {
  id: string;
  name: string;
  email: string;
  status: UserStatus;
  tier: string;
  sessions: number;
  weights: number;
  lastSeen: string;
  emailConfirmed: boolean;
}
export interface UserAdminStats {
  activeUsers: number;
  activeUsersGrowth: number;
  suspendedUsers: number;
  suspendedGrowth: number;
  emailConfirmedRate: number;
  passwordResets7d: number;
  reviewQueueCount: number;
  syncGapsCount: number;
  dormantEliteCount: number;
}

export interface UserAuditLog {
  date: string;
  action: string;
}

export interface UserDetail extends UserRow {
  roles: string[];
  lastActive: string;
  createdAt: string;
  supportPriority: 'Low' | 'Medium' | 'High';
  retentionFlag: 'Stable' | 'At Risk' | 'Dormant';
  auditTrail: UserAuditLog[];
}
