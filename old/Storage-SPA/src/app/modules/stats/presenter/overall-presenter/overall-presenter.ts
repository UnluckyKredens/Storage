import { Statistics } from '../../../../shared/entities/statistics.model';
import { StatsService } from './../../services/stats.service';
import { Component } from '@angular/core';

@Component({
  selector: 'app-overall-presenter',
  standalone: false,
  templateUrl: './overall-presenter.html',
  styleUrl: './overall-presenter.scss'
})
export class OverallPresenter {

  stats: Statistics | undefined;

  constructor(private readonly StatsService: StatsService) {
    this.getStats();
  }

  getStats() {
    this.StatsService.getStats().subscribe((s) =>
      this.stats = s
    );
  }

}
