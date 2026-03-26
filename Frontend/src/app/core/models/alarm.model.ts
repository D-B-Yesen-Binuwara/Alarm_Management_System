export interface Alarm {
  alarmId: number;
  deviceId: number;
  alarmType: string;
  raisedTime: string;
  clearedTime?: string;
  isActive: boolean;
}
