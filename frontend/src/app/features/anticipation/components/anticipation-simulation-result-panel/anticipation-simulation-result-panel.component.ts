import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnticipationSimulation } from '../../../../domain';

@Component({
  selector: 'app-anticipation-simulation-result-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './anticipation-simulation-result-panel.component.html',
  styleUrl: './anticipation-simulation-result-panel.component.scss'
})
export class AnticipationSimulationResultPanelComponent {

  @Input({ required: true }) simulationResult!: AnticipationSimulation;
  @Input() canConvert = false;
  @Input() userRole = 'Creator';
  @Input() isConverting = false;
  @Input() simulatedForCreator?: string; // CA-RF3-6: for admin label

  @Output() convertClick = new EventEmitter<void>();

  onConvert(): void {
    this.convertClick.emit();
  }

  formatCurrency(cents: number): string {
    return (cents / 100).toLocaleString('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    });
  }

  formatDate(date: Date | undefined): string {
    if (!date) return '—';
    return date.toLocaleDateString('pt-BR');
  }

  formatDateTime(date: Date | undefined): string {
    if (!date) return '—';
    return date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
  }

  /** Returns true when less than 10 minutes remain (for warning styling). */
  isExpiringSoon(): boolean {
    if (!this.simulationResult) return false;
    const remaining = this.simulationResult.validUntil.getTime() - Date.now();
    return remaining > 0 && remaining < 10 * 60 * 1000;
  }
}

