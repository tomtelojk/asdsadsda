# Validação da edição 7

- `scripts/package.sh`: build Release sem avisos; 1.217 verificações aprovadas; publicação autocontida Linux x86_64.
- Executável distribuído: `--self-test` aprovou as mesmas 1.217 verificações e `--version` informou 7.0.0.
- Percurso com janela real no Xvfb e entradas XTest: tela inicial, duas páginas da carta, diário, plantar/regar, entrar em casa, dormir, viajar, conversar, assunto do caderno, comprar sementes e entrar/sair dos sete interiores.
- Campanha na interface: seis partidas de verificação com pré-requisitos preparados, entrega em cada capítulo, consumo/recompensas e cartas persistidos. Os testes de regras também percorrem o cultivo e a campanha completa sem depender desses saves preparados.
- Captura de 20 segundos do executável: os oito moradores mudaram de posição ou área. GIF e capturas do README vêm dessa versão.
- Regressão encontrada no percurso: NPC próximo da soleira podia tomar a interação da porta. Acesso à porta corrigido e coberto por teste.

Cobertura inclui recortes dentro dos PNGs distribuídos, proporção e âncoras de todos os objetos catalogados, margens dos retratos, câmera/viewport e inversão do mouse, navegação, limites numéricos, migração e validação dos saves, história e diagramação de textos. Os workflows verificam novamente o código e abrem as nove áreas.

O teste gráfico usa renderização Mesa no Linux. Compatibilidade com outras distribuições, drivers e monitores depende dos requisitos documentados no README.
