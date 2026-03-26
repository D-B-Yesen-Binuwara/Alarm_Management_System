import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DeviceService } from '../../core/services/device.service';
import { AlarmService } from '../../core/services/alarm.service';
import { Device } from '../../core/models/device.model';
import { Alarm } from '../../core/models/alarm.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './dashboard.html'
})
export class DashboardComponent implements OnInit {
  devices = signal<Device[]>([]);
  alarms = signal<Alarm[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  lastRefreshed = signal<Date>(new Date());

  // Filters
  searchTerm = '';
  selectedType = '';
  selectedStatus = '';

  deviceTypes = ['SLBN', 'CEAN', 'MSAN', 'Customer'];
  statuses = ['UP', 'DOWN'];

  private readonly deviceTypeLabels: Record<number, string> = {
    0: 'SLBN',
    1: 'CEAN',
    2: 'MSAN',
    3: 'Customer'
  };

  private readonly priorityLabels: Record<number, string> = {
    0: 'Low',
    1: 'Medium',
    2: 'High',
    3: 'Critical'
  };

  constructor(
    private deviceService: DeviceService,
    private alarmService: AlarmService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.deviceService.getAll().subscribe({
      next: (data) => {
        this.devices.set(data);
        this.loading.set(false);
        this.lastRefreshed.set(new Date());
      },
      error: (err) => {
        this.error.set('Failed to load devices. Is the API running?');
        this.loading.set(false);
      }
    });

    this.alarmService.getAll().subscribe({
      next: (data) => this.alarms.set(data)
    });
  }

  refresh(): void {
    this.load();
  }

  get totalNodes(): number {
    return this.devices().length;
  }

  get activeNodes(): number {
    return this.devices().filter(d => this.isUpStatus(d.status)).length;
  }

  get failedNodes(): number {
    return this.devices().filter(d => this.isDownStatus(d.status)).length;
  }

  get activeAlarms(): number {
    return this.alarms().filter(a => a.isActive).length;
  }

  get recentAlarms(): Alarm[] {
    return this.alarms()
      .filter(a => a.isActive)
      .sort((a, b) => new Date(b.raisedTime).getTime() - new Date(a.raisedTime).getTime())
      .slice(0, 5);
  }

  get filteredDevices(): Device[] {
    const term = this.searchTerm.toLowerCase();
    return this.devices().filter(d => {
      const matchesSearch =
        !term ||
        d.deviceName?.toLowerCase().includes(term) ||
        d.ip?.toLowerCase().includes(term);
      const matchesType = !this.selectedType || this.getDeviceTypeLabel(d.deviceType) === this.selectedType;
      const matchesStatus = !this.selectedStatus || this.normalizeStatus(d.status) === this.selectedStatus;
      return matchesSearch && matchesType && matchesStatus;
    });
  }

  getDeviceNameForAlarm(deviceId: number): string {
    return this.devices().find(d => d.deviceId === deviceId)?.deviceName ?? `Device #${deviceId}`;
  }

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleString('en-GB', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit', second: '2-digit'
    });
  }

  getAlarmRowClass(alarmType: string): string {
    const t = alarmType?.toUpperCase();
    if (t === 'NODE_DOWN') return 'bg-red-50 border-l-4 border-red-400';
    if (t === 'AC') return 'bg-orange-50 border-l-4 border-orange-400';
    return 'bg-yellow-50 border-l-4 border-yellow-400';
  }

  getAlarmBadgeClass(alarmType: string): string {
    const t = alarmType?.toUpperCase();
    if (t === 'NODE_DOWN') return 'bg-red-200 text-red-800';
    if (t === 'AC') return 'bg-orange-200 text-orange-800';
    return 'bg-yellow-200 text-yellow-800';
  }

  getStatusBadgeClass(status: string): string {
    return this.isUpStatus(status)
      ? 'bg-green-100 text-green-800'
      : 'bg-red-100 text-red-800';
  }

  getTypeBadgeClass(deviceType: string | number): string {
    const label = this.getDeviceTypeLabel(deviceType);
    const map: Record<string, string> = {
      SLBN: 'bg-purple-100 text-purple-800',
      CEAN: 'bg-blue-100 text-blue-800',
      MSAN: 'bg-teal-100 text-teal-800',
      Customer: 'bg-gray-100 text-gray-800'
    };
    return map[label] ?? 'bg-gray-100 text-gray-800';
  }

  getPriorityBadgeClass(priority: string | number): string {
    const label = this.getPriorityLabel(priority);
    const map: Record<string, string> = {
      Critical: 'bg-red-100 text-red-800',
      High: 'bg-orange-100 text-orange-800',
      Medium: 'bg-yellow-100 text-yellow-800',
      Low: 'bg-green-100 text-green-800'
    };
    return map[label] ?? 'bg-gray-100 text-gray-700';
  }

  hasAlarms(deviceId: number): boolean {
    return this.alarms().some(a => a.deviceId === deviceId && a.isActive);
  }

  getDeviceTypeLabel(deviceType: string | number): string {
    if (typeof deviceType === 'number') {
      return this.deviceTypeLabels[deviceType] ?? String(deviceType);
    }

    return deviceType;
  }

  getPriorityLabel(priority: string | number): string {
    if (typeof priority === 'number') {
      return this.priorityLabels[priority] ?? String(priority);
    }

    return priority;
  }

  normalizeStatus(status: string | number | null | undefined): string {
    const normalized = String(status ?? '').trim().toUpperCase();

    if (this.isUpStatus(normalized)) {
      return 'UP';
    }

    if (this.isDownStatus(normalized)) {
      return 'DOWN';
    }

    return normalized;
  }

  private isUpStatus(status: string | number | null | undefined): boolean {
    const value = String(status ?? '').trim().toUpperCase();
    return value === 'UP' || value === 'ONLINE' || value === 'ALIVE' || value === '1' || value === 'TRUE';
  }

  private isDownStatus(status: string | number | null | undefined): boolean {
    const value = String(status ?? '').trim().toUpperCase();
    return value === 'DOWN' || value === 'OFFLINE' || value === 'DEAD' || value === 'FAILED' || value === '0' || value === 'FALSE';
  }
}
