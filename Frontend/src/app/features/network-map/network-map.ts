import { Component, AfterViewInit, ElementRef, OnDestroy, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';
import { DeviceMapPoint, DeviceService } from '../../core/services/device.service';

@Component({
  selector: 'app-network-map',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './network-map.html'
})
export class NetworkMapComponent implements AfterViewInit, OnDestroy {
  @ViewChild('mapContainer', { static: true }) mapContainer!: ElementRef<HTMLDivElement>;

  loading = signal(true);
  error = signal<string | null>(null);

  private map: L.Map | null = null;
  private markerLayer = L.layerGroup();
  private pendingDevices: DeviceMapPoint[] = [];
  private readonly markerShadowUrl = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png';
  private readonly greenMarkerIconUrl = 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png';
  private readonly redMarkerIconUrl = 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png';
  private readonly yellowMarkerIconUrl = 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-yellow.png';

  constructor(private deviceService: DeviceService) {}

  ngAfterViewInit(): void {
    this.loading.set(true);
    this.initializeMap();
    this.loadMapDevices();
  }

  ngOnDestroy(): void {
    if (this.map) {
      this.map.remove();
      this.map = null;
    }
  }

  private initializeMap(): void {
    if (this.map) {
      return;
    }

    this.map = L.map(this.mapContainer.nativeElement).setView([7.8731, 80.7718], 7);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.map);

    this.markerLayer.addTo(this.map);

    if (this.pendingDevices.length > 0) {
      this.plotMarkers(this.pendingDevices);
      this.pendingDevices = [];
    }

    setTimeout(() => this.map?.invalidateSize(), 0);
  }

  private loadMapDevices(): void {
    this.deviceService.getDevicesForMap().subscribe({
      next: (devices) => {
        if (this.map) {
          this.plotMarkers(devices);
          this.map.invalidateSize();
        } else {
          this.pendingDevices = devices;
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load map devices. Is the API running?');
        this.loading.set(false);
      }
    });
  }

  private plotMarkers(devices: DeviceMapPoint[]): void {
    this.markerLayer.clearLayers();

    for (const device of devices) {
      L.marker([device.latitude, device.longitude], {
        icon: this.getMarkerIcon(device.status, device.isImpacted === 1)
      })
        .bindPopup(
          `Device ID: ${device.deviceId}<br>Device Name: ${device.deviceName}<br>Device Type: ${device.deviceType}<br>Status: ${this.normalizeStatus(device.status)}`
        )
        .addTo(this.markerLayer);
    }
  }

  private getMarkerIcon(status: string, isImpacted: boolean): L.Icon {
    const iconUrl = isImpacted
      ? this.yellowMarkerIconUrl
      : this.normalizeStatus(status) === 'DOWN'
        ? this.redMarkerIconUrl
        : this.greenMarkerIconUrl;

    return L.icon({
      iconUrl,
      shadowUrl: this.markerShadowUrl,
      iconSize: [25, 41],
      iconAnchor: [12, 41],
      popupAnchor: [1, -34],
      shadowSize: [41, 41]
    });
  }

  private normalizeStatus(status: string | number | null | undefined): string {
    return String(status ?? '').toUpperCase();
  }
}
