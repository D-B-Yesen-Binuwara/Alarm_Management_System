export interface Device {
  deviceId: number;
  deviceName: string;
  deviceType: string; // 'SLBN' | 'CEAN' | 'MSAN' | 'Customer'
  ip: string;
  status: string;    // 'UP' | 'DOWN'
  priorityLevel: string;  // 'Low' | 'Medium' | 'High' | 'Critical'
  leaId: number;
  assignedUserId?: number;
}
