export type UserStatus = 'Active' | 'Review' | 'Suspended';

export interface UserRow {
  id: string;
  userName: string;
  email: string;
  emailConfirmed: boolean;
  phoneNumber?: string;
  phoneNumberConfirmed: boolean;
  twoFactorEnabled: boolean;
  lockoutEnd?: string | null;
  accessFailedCount: number;

  name: string;
  status: UserStatus;
  sessions: number;
  weights: number;
  /** ISO 8601 datetime string, e.g. 2026-04-08T14:22:31.123Z */
  lastSeen: string;
  tier: 'Free' | 'Basic' | 'Pro' | 'Elite';
  dailyWeights?: number;
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
