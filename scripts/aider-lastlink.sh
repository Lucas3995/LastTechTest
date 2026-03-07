#!/bin/bash

export OLLAMA_API_BASE=http://localhost:11434

cd /media/belo/BeloSSD_2/Processos_Sel/LastLink/LastTechTest/

aider \
  --model ollama/qwen3:8b \
  --no-auto-commits \
  --read README.md \
  --read .cursor/skills/quadro-de-recompensas/SKILL.md \
  --read .cursor/plans/plano-arvore-testes_RO-1-observabilidade-opentelemetry-serilog.plan.md \
  --message "Leia o README.md para ter um overview do projeto. Em seguida, siga as instruções do arquivo .cursor/skills/quadro-de-recompensas/SKILL.md tendo como fonte de verdade o conteúdo do arquivo .cursor/plans/plano-arvore-testes_RO-1-observabilidade-opentelemetry-serilog.plan.md. Considere que parte do trabalho já foi feita — analise o que falta antes de começar qualquer implementação. Siga metodologia spec-driven: não implemente nada que não esteja especificado."