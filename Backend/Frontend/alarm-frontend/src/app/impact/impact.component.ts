import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-impact',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './impact.component.html',
  styleUrls: ['./impact.component.css']
})
export class ImpactComponent implements OnInit {

  devices: any[] = [];
  deviceId: number = 0;

  failedDevice: string = '';
  totalImpacted: number = 0;

  impactedDevices: any[] = [];

  ngOnInit(): void {

    fetch("http://localhost:5289/api/device")
      .then(res => res.json())
      .then(data => {
        console.log("Devices from API:", data);
        this.devices = data;
      })
      .catch(error => {
        console.error("Device API error:", error);
      });

  }

  runImpact(): void {

    if (!this.deviceId) {
      alert("Please select a device");
      return;
    }

    fetch("http://localhost:5289/api/impact/" + this.deviceId)
      .then(res => res.json())
      .then(data => {

        console.log("Impact Result:", data);

        this.failedDevice = data.failedDevice;
        this.totalImpacted = data.totalImpacted;
        this.impactedDevices = data.impactedDevices;

      })
      .catch(error => {
        console.error("Impact API error:", error);
      });

  }

}
