import { Component } from '@angular/core';
import { ConnectionService } from '../../services/connection.service';
import { Subscriber } from 'rxjs';

@Component({
  selector: 'app-storage-container',
  standalone: false,
  templateUrl: './storage-container.html',
  styleUrl: './storage-container.scss'
})
export class StorageContainer {
  connection: boolean = false;
  constructor(private readonly connectionService: ConnectionService) {}


  ngOnInit(): void {
  this.connection = this.connectionService.checkConncection();  }

}
