# Prism Adoption in Nintex

Which teams are using Prism, how they are using it and who is championing it.  

## 📣Champions📣

- Greg Richardson - AO - Melbourne
- Johan Coetzer - EM - Melbourne
- JD Bell - AO - Melbourne
- Riaan Nagel - AO - South Africa
- Rick De Marco - PM - Melbourne

## Team Prism

- Contacts: Greg Richardson (AO), Johan Coetzer (EM), JD Bell (AO), Riaan Nagel (AO), Rick De Marco (PM)
- Product: Prism. 🌀*Yes - Prism is installed into `rnd-prism` repo and used to develop Prism!*🌀
- Repo: (single-repo) [rnd-prism](https://github.com/NintexGlobal/rnd-prism)
- Status: (17-Jul-26) Full use of framework

## Space Invaders

- Contacts: Greg Richardson (AO), Johan Coetzer (EM)
- Products: Tables, Files
- Repos: (multi-repo) [space-invaders-specs](https://nintex.visualstudio.com/Nintex/_git/space-invaders-specs) (prism SDD), [ntx-tables-api](https://nintex.visualstudio.com/Nintex/_git/ntx-tables-api), [ntx-tables-client](https://nintex.visualstudio.com/Nintex/_git/ntx-tables-client), [ntx-tables-orchestration-service](https://nintex.visualstudio.com/Nintex/_git/ntx-tables-orchestration-service), [ntx-files](https://nintex.visualstudio.com/Nintex/_git/ntx-files)
- Status: (17-Jul-26) Full use of framework

## Thunderbirds

- Contacts: Fawad Ahmed (EM), Salman Aziz (Eng), Ignacio Borello (Eng)
- Product: AI Assistant
- Repo: (single-repo) [nac-copilot](https://github.com/NintexGlobal/nac-copilot) mono repo
- Status: (17-Jul-26) Active use (talk to Fawad for more details)

## Harmonizers

- Contacts:  Philip Ong (EM), JD Bell (AO), Raffles Gun (ENG)
- Product: Fantastic 8 Connectors
  - Repo: (multi-repo) [int-nxc-middleware-specs](https://github.com/NintexGlobal/int-nxc-middleware-specs) (prism SDD), [int-nxc-ukg](https://github.com/NintexGlobal/int-nxc-ukg)
  - Status: (17-Jul-26) New, starting use.
- Product: Connector acceleration
  - Repos: [int-nxc-middleware-specs](https://github.com/NintexGlobal/int-nxc-middleware-specs) (prism SDD), [int-nxc-snowflake](https://github.com/NintexGlobal/int-nxc-snowflake)
  - Status (9/9/26) using BMAD to generate connectors (based on early work by Joshua Hoogerwerf that was very successfully generating connectors using BMAD). Using Prism to create and maintain a new microservice.

## Phoenix

- Contacts: Marvin Tan (EM), JD Bell (AO), Samuel Tan (ENG)
- Product: Integration Hub (subset of Phoenix)
- Repo: (single-repo) [int-nxt-xtensions-objects](https://github.com/NintexGlobal/int-nxt-xtensions-objects) mono repo
- Status: (17-Jul-26) Existing, currently adopting/ramping Prism (installed 15 July)

## Nexus (restructuring...)

- Contacts: Philip Ong (EM) (but changing), Ghee Yuen Teoh (ENG) / Praveen Gunasekren (ENG)
- Products: All Nexus work: ALM deployment, Topology/dependency service
- Repos: (multi-repo) [rnd-nexus-prism](https://github.com/NintexGlobal/rnd-nexus-prism) (prism SDD), [ntx-alm-deployment](https://github.com/NintexGlobal/ntx-alm-deployment), [ntx-topology](https://github.com/NintexGlobal/ntx-topology), (also considering [nwc-dashboard](https://nintex.visualstudio.com/Nintex/_git/nwc-dashboard) for their UI elements)
- Status: (17-Jul-26) Exploration

## Monyets - Tasks, Designer, Engine

- Contacts: Adrian Choo (EM)
- Product: Tasks Service
- Repo: [nwc-tasks-svc](https://nintex.visualstudio.com/Nintex/_git/nwc-tasks-svc), `...action service`, [nwc-designer](https://nintex.visualstudio.com/Nintex/_git/nwc-designer), `...engine`, [the-monyets-prism](https://nintex.visualstudio.com/Nintex/_git/the-monyets-prism)
- Status: (6-Aug-26) Only using the generate-context-repo skill ATM! Not Prism SDD yet. There are plans to also start using Prism.

## Monyets - Ingress

- Contacts: Adrian Choo (EM), Arif Shamsudin (ENG)
- Product: NWC Engine Ingress Service
- Repos: [monyets-egress-prism](https://nintex.visualstudio.com/Nintex/_git/monyets-egress-prism) (prism SDD), [nwc-engine-egress-svc](https://nintex.visualstudio.com/Nintex/_git/nwc-engine-ingress-svc), ... as above, 
- Status: (17-Jul-26) Only using the generate-context-repo skill ATM! Not Prism SDD yet. There are plans to also start using Prism.

## Fabers (restructuring...)

- Contacts: Kok Fong (EM), Akshay (PM)
- Product: Platform Experience
  - Repos: [nwc-dashboard](https://nintex.visualstudio.com/Nintex/_git/nwc-dashboard), [ntx-platform-container-svc](https://nintex.visualstudio.com/Nintex/_git/ntx-platform-container-svc)
  - Status: (9/9/26) Adopting
- Product: ALM
  - Repos: [ntx-alm-packaging](https://github.com/NintexGlobal/ntx-alm-packaging), [ntx-alm-deployment](https://github.com/NintexGlobal/ntx-alm-deployment)
  - Status: (9/9/26) Adopting

## Data Alchemists

- Contacts: Ruan de Villiers (ENG), Vignesh Iyer (ENG), Phalardira Moja (ENG), Orcun Danismaz (ENG)
- Products:
- Repos: [ana-analytics](https://github.com/NintexGlobal/ana-analytics),  [poc-ai-optimization](https://github.com/NintexGlobal/poc-ai-optimization)
- Status: (17-Jul-26) Investigating

## Argonauts

- Contacts: Jason Spangler (ENG), Lewis Garmston (PM)
- Product: Nintex Apps
- Repos: ([github Skuid](https://github.com/skuid/)) skuid-monorepo, pliny, warden, nautilus, seaquill
- Status: Investigating

## Agentrix

- Contacts: Andrew Dongo (EM), Kok Jun Lye (ENG) (Note that Agentrix is a virtual team made of of Genkiflo, Pirates and Binary)
- Product: Agents Designer
- Repos: [agt-agents](https://github.com/NintexGlobal/agt-agents)
- Status: (3/8/26) Pilot sprint using Prism

## Pirates (Agentrix)

- Contacts: Naveen (EM), Matt McGowan, Vigar Block
- Product: NWC Designer
  - Repos: [agt-agentrix-specs](https://github.com/NintexGlobal/agt-agentrix-specs) (prism SDD), [nwc-designer](https://nintex.visualstudio.com/Nintex/_git/nwc-designer)
  - Status: (9/9/26) Adopting
- Product: Agents Design Service
  - Repos: [agt-agentrix-specs](https://github.com/NintexGlobal/agt-agentrix-specs) (prism SDD), [agt-orchestration-design](https://github.com/NintexGlobal/agt-orchestration-design)
  - Status: (9/9/26) Adopting

## Outliers

- Contacts: Leslie, Aymen
- Product: Zinc Core
- Repos: [rnd-outliers-prism](https://github.com/NintexGlobal/rnd-outliers-prism) (prism SDD), [zinc-core](https://nintex.visualstudio.com/Nintex/_git/zinc-core)
- Status: (9/9/26) Using Prism to generate features. Previously used [SuperPowers](https://github.com/obra/superpowers)

## Voltron

- Contacts: Bond Lim (EM), Peter Byun (PM)
- Product: 
- Repos:
- Status: (18/9/26) Installing and investigating

## To check 
Pawan: Forms
