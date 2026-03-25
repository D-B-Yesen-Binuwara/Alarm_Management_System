import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-impact',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './impact.html',
  styleUrls: ['./impact.css']
})
export class ImpactComponent implements OnInit {

  devices:any[] = [];
  impactedDevices:any[] = [];

  deviceId:number = 0;

  failedDevice:string = "-";
  impactCount:number = 0;

  constructor(private http:HttpClient){}

  ngOnInit(){
    this.loadDevices();
  }

  loadDevices(){

    this.http.get<any>('http://localhost:5289/api/device')
    .subscribe(data=>{

      this.devices = data;

      if(this.devices.length>0){
        this.deviceId = this.devices[0].id;
      }

    });

  }

  runImpact(){

    this.http.get<any>('http://localhost:5289/api/Impact/'+this.deviceId)
    .subscribe(data=>{

      this.impactedDevices = data;

      this.impactCount = data.length;

      if(data.length>0){
        this.failedDevice = data[0].deviceName;
      }

    });

  }

}
