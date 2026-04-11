export interface UserAccessConfig {
  requireAdminApprovalForCoaches: boolean;
  allowCoachesExportData: boolean;
  allowAthletesEditHistory: boolean;
  guestViewOnlyMode: boolean;
  maxLoginAttempts: number;
  lockoutDurationMinutes: number;
  sendEmailAlertOnLockout: boolean;
}

export const defaultUserAccessConfig: UserAccessConfig = {
  requireAdminApprovalForCoaches: true,
  allowCoachesExportData: false,
  allowAthletesEditHistory: true,
  guestViewOnlyMode: true,
  maxLoginAttempts: 5,
  lockoutDurationMinutes: 30,
  sendEmailAlertOnLockout: true,
};
