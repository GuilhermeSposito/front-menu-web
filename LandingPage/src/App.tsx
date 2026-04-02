/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
*/

import React from "react";
import { motion } from "motion/react";
import {
  ChefHat,
  Store,
  BarChart3,
  Smartphone,
  ShieldCheck,
  Headphones,
  Utensils,
  Coffee,
  ShoppingBag,
  CheckCircle2,
  ArrowRight,
  Menu,
  X,
  MapPin,
  Users,
  Globe,
  Handshake
} from "lucide-react";
import { useState, useEffect, useRef } from "react";

type FormStatus = 'idle' | 'loading' | 'success' | 'error';

async function gerarHashHmac(secret: string, timestamp: string): Promise<string> {
  const encoder = new TextEncoder();
  const key = await crypto.subtle.importKey(
    'raw',
    encoder.encode(secret),
    { name: 'HMAC', hash: 'SHA-256' },
    false,
    ['sign']
  );
  const signature = await crypto.subtle.sign('HMAC', key, encoder.encode(timestamp));
  return Array.from(new Uint8Array(signature))
    .map(b => b.toString(16).padStart(2, '0'))
    .join('');
}

export default function App() {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [empresa, setEmpresa] = useState('');
  const [email, setEmail] = useState('');
  const [whatsapp, setWhatsapp] = useState('');
  const [formStatus, setFormStatus] = useState<FormStatus>('idle');
  const [formMessage, setFormMessage] = useState('');

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setFormStatus('loading');
    setFormMessage('');

    const apiUrl = import.meta.env.VITE_API_URL;
    const apiKey = import.meta.env.VITE_API_KEY;
    const hmacSecret = import.meta.env.VITE_HMAC_SECRET;

    const timestamp = String(Date.now());
    const hash = await gerarHashHmac(hmacSecret, timestamp);

    try {
      const res = await fetch(`${apiUrl}/envios-email/comercial`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'x-api-key': apiKey,
          'x-timestamp': timestamp,
          'x-hash': hash,
        },
        body: JSON.stringify({
          EmailCliente: email,
          NomeCliente: empresa,
          Assunto: `Solicitação de contato - ${empresa}`,
          Conteudo: `Empresa: ${empresa}\nWhatsApp: ${whatsapp}`,
        }),
      });

      if (res.ok) {
        setFormStatus('success');
        setFormMessage('Solicitação enviada com sucesso!');
        setEmpresa('');
        setEmail('');
        setWhatsapp('');
      } else {
        setFormStatus('error');
        setFormMessage('Erro ao enviar. Tente novamente.');
      }
    } catch {
      setFormStatus('error');
      setFormMessage('Erro de conexão. Tente novamente.');
    }
  }


  const segments = [
    { icon: <Utensils className="w-8 h-8" />, name: "Restaurantes", desc: "Gestão completa de mesas, comandas e cozinha." },
    { icon: <Coffee className="w-8 h-8" />, name: "Bares & Cafés", desc: "Agilidade no atendimento e controle de estoque rigoroso." },
    { icon: <ShoppingBag className="w-8 h-8" />, name: "Mercados", desc: "PDV rápido e integração com balanças e etiquetas." },
    { icon: <Store className="w-8 h-8" />, name: "Conveniências", desc: "Controle de vendas 24h com facilidade e segurança." },
  ];

  const features = [
    { title: "Controle de Estoque", desc: "Gestão inteligente de insumos e produtos acabados com alertas de reposição.", icon: <BarChart3 /> },
    { title: "PDV Ágil", desc: "Frente de caixa intuitivo e integrado para reduzir filas e erros.", icon: <Smartphone /> },
    { title: "Financeiro Completo", desc: "Fluxo de caixa, contas a pagar/receber e conciliação bancária em um só lugar.", icon: <ShieldCheck /> },
    { title: "Relatórios Estratégicos", desc: "Dados em tempo real para tomadas de decisão baseadas em fatos.", icon: <ChefHat /> },
  ];

  const clientes = [
    { nome: "Restaurante Piassa", segmento: "Restaurante", foto: "images/empresas-nossaas/piassa.jpeg" },
    { nome: "Macarrão Piassa", segmento: "Restaurante", foto: "images/empresas-nossaas/macarraopiassa.jpeg" },
    { nome: "Pastel da Li", segmento: "Lanchonete", foto: "images/empresas-nossaas/PastelDaLi.png" },
    { nome: "Burgues on the Table", segmento: "Hamburgueria", foto: "images/empresas-nossaas/burguers-on-the-table.png" },
    { nome: "Pão com Gergelim", segmento: "Lanchonete", foto: "images/empresas-nossaas/pao-com-gergelim.jpg" },
  ];

  // Adicione mais imagens em images/ e inclua aqui para expandir o carrossel
  const dashboardSlides = [
    { src: "images/dashboard-image.png", label: "Dashboard principal" },
    { src: "images/demonstracao.prod.page.png", label: "Controle de Estoque" },
    // { src: "images/dashboard-financeiro.png", label: "Financeiro" },
  ];

  const [activeDashboard, setActiveDashboard] = useState(0);
  const dashboardIntervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    if (dashboardSlides.length <= 1) return;
    dashboardIntervalRef.current = setInterval(() => {
      setActiveDashboard((prev) => (prev + 1) % dashboardSlides.length);
    }, 4000);
    return () => {
      if (dashboardIntervalRef.current) clearInterval(dashboardIntervalRef.current);
    };
  }, [dashboardSlides.length]);

  function goToDashboardSlide(idx: number) {
    setActiveDashboard(idx);
    if (dashboardIntervalRef.current) clearInterval(dashboardIntervalRef.current);
    if (dashboardSlides.length > 1) {
      dashboardIntervalRef.current = setInterval(() => {
        setActiveDashboard((prev) => (prev + 1) % dashboardSlides.length);
      }, 4000);
    }
  }

  const cardapioSlides = [
    { src: "images/DemostracaoCardapioDigitalSophos/1.jpeg" },
    { src: "images/DemostracaoCardapioDigitalSophos/2.jpeg" },
    { src: "images/DemostracaoCardapioDigitalSophos/3.jpeg" },
  ];

  const [activeCardapio, setActiveCardapio] = useState(0);
  const cardapioIntervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    cardapioIntervalRef.current = setInterval(() => {
      setActiveCardapio((prev) => (prev + 1) % cardapioSlides.length);
    }, 3500);
    return () => {
      if (cardapioIntervalRef.current) clearInterval(cardapioIntervalRef.current);
    };
  }, [cardapioSlides.length]);

  function goToCardapioSlide(idx: number) {
    setActiveCardapio(idx);
    if (cardapioIntervalRef.current) clearInterval(cardapioIntervalRef.current);
    cardapioIntervalRef.current = setInterval(() => {
      setActiveCardapio((prev) => (prev + 1) % cardapioSlides.length);
    }, 3500);
  }

  const [activeCliente, setActiveCliente] = useState(0);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    intervalRef.current = setInterval(() => {
      setActiveCliente((prev) => (prev + 1) % clientes.length);
    }, 3500);
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [clientes.length]);

  return (
    <div className="min-h-screen bg-sophos-bg font-sans text-slate-100 overflow-x-hidden">
      {/* Navbar */}
      <nav className="fixed top-0 w-full bg-sophos-primary backdrop-blur-md z-50 border-b border-slate-800">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-20 items-center">
            <div className="flex items-center gap-2">
              <img
                width={30}
                height={30}
                src="images/CEREBRO-VETOR.svg"
                alt="Dashboard do Sophos ERP"
                className="rounded-xl opacity-100"
                referrerPolicy="no-referrer"
              />

              <span className="text-2xl font-bold tracking-tight text-white">Sophos</span>
            </div>

            {/* Desktop Menu */}
            <div className="hidden md:flex items-center gap-8">
              <a href="#recursos" className="text-slate-300 hover:text-sophos-primary transition-colors font-medium">Recursos</a>
              <a href="#segmentos" className="text-slate-300 hover:text-sophos-primary transition-colors font-medium">Segmentos</a>
              <a href="#cardapio-digital" className="text-slate-300 hover:text-sophos-primary transition-colors font-medium">Cardápio Digital</a>
              <a href="#integracoes" className="text-slate-300 hover:text-sophos-primary transition-colors font-medium">Integrações</a>
              <a href="#sobre" className="text-slate-300 hover:text-sophos-primary transition-colors font-medium">Sobre</a>
              <button onClick={() => window.open("https://wa.me/5516992366175", "_blank")} className="bg-sophos-primary text-white px-6 py-2.5 rounded-full font-semibold hover:opacity-90 transition-all shadow-lg shadow-sophos-primary/20">
                Falar com Consultor
              </button>
              <a href="https://sophos-erp.com.br/login" target="_blank" rel="noopener noreferrer" className="bg-sophos-card text-sophos-primary hover:opacity-80 px-5 py-2.5 rounded-full font-semibold transition-all border border-slate-700">
                Entrar
              </a>
            </div>

            {/* Mobile Menu Toggle */}
            <div className="md:hidden">
              <button onClick={() => setIsMenuOpen(!isMenuOpen)} className="p-2 text-white">
                {isMenuOpen ? <X /> : <Menu />}
              </button>
            </div>
          </div>
        </div>

        {/* Mobile Menu */}
        {isMenuOpen && (
          <motion.div
            initial={{ opacity: 0, y: -20 }}
            animate={{ opacity: 1, y: 0 }}
            className="md:hidden bg-sophos-card border-b border-slate-800 p-4 flex flex-col gap-4"
          >
            <a href="#recursos" onClick={() => setIsMenuOpen(false)} className="text-slate-300 font-medium">Recursos</a>
            <a href="#segmentos" onClick={() => setIsMenuOpen(false)} className="text-slate-300 font-medium">Segmentos</a>
            <a href="#cardapio-digital" onClick={() => setIsMenuOpen(false)} className="text-slate-300 font-medium">Cardápio Digital</a>
            <a href="#integracoes" onClick={() => setIsMenuOpen(false)} className="text-slate-300 font-medium">Integrações</a>
            <a href="#sobre" onClick={() => setIsMenuOpen(false)} className="text-slate-300 font-medium">Sobre</a>
            <button onClick={() => window.open("https://wa.me/5516992366175", "_blank")} className="bg-sophos-primary text-white px-6 py-3 rounded-xl font-semibold">Falar com Consultor</button>
            <a href="https://sophos-erp.com.br/login" target="_blank" rel="noopener noreferrer" className="text-center bg-sophos-card text-sophos-primary border border-slate-700 px-6 py-3 rounded-xl font-semibold">Entrar</a>
          </motion.div>
        )}
      </nav>

      {/* Hero Section */}
      <section className="pt-32 pb-20 lg:pt-48 lg:pb-32 px-4 relative overflow-hidden">
        <div className="absolute top-0 right-0 -z-10 w-1/2 h-full bg-sophos-primary/5 rounded-bl-[200px]" />
        <div className="max-w-7xl mx-auto grid lg:grid-cols-2 gap-12 items-center">
          <motion.div
            initial={{ opacity: 0, x: -50 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.6 }}>
            <span className="inline-block py-1 px-3 rounded-full bg-sophos-primary/20 text-sophos-primary text-sm font-bold mb-6 tracking-wider uppercase">
              ERP · CRM · Cardápio Digital · PDV · Consultoria de TI
            </span>
            <h1 className="text-5xl lg:text-6xl font-extrabold text-white leading-[1.1] mb-6">
              Tudo que sua empresa precisa em <span className="text-sophos-primary">um único sistema</span> —
            </h1>
            <p className="text-lg text-slate-400 mb-8 leading-relaxed max-w-xl">
              O Sophos vai muito além de um ERP. Entregamos <span className="text-white font-semibold">CRM, mensagens automáticas de status de pedido, cardápio digital para Delivery, Balcão, Mesas e QR Code, PDV para mercados</span>, <span className="text-white font-semibold">controle financeiro completo</span> com fluxo de caixa, contas a pagar e receber, e <span className="text-white font-semibold">gestão de estoque inteligente</span> com alertas de reposição. Cada operador trabalha com seu <span className="text-white font-semibold">caixa individual</span>, garantindo fechamentos precisos e rastreabilidade total por usuário — e ainda cuidamos da sua infraestrutura: impressoras, redes, dispositivos locais e toda a <span className="text-sophos-primary font-semibold">consultoria de TI</span> que sua operação exige.
            </p>

            {/* Feature pills */}
            <div className="flex flex-wrap gap-2 mb-10">
              {[
                'ERP Completo',
                'CRM',
                'Controle Financeiro',
                'Gestão de Estoque',
                'Caixa por Usuário',
                'Delivery',
                'Balcão',
                'Mesas & QR Code',
                'PDV Mercado',
                'Mensagem Automática de Status',
                'Impressoras & Dispositivos',
                'Consultoria de TI',
              ].map((item) => (
                <span key={item} className="px-3 py-1 rounded-full bg-sophos-card border border-slate-700 text-slate-300 text-xs font-medium">
                  {item}
                </span>
              ))}
            </div>

            <div className="flex flex-col sm:flex-row gap-4">
              <button onClick={() => window.open("https://wa.me/5516992366175", "_blank")} className="bg-sophos-primary text-white px-8 py-4 rounded-full font-bold text-lg hover:opacity-90 transition-all flex items-center justify-center gap-2 shadow-xl shadow-sophos-primary/20 group">
                Solicitar Demonstração Gratuita
                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
              </button>
            </div>

            <div className="mt-10 flex flex-wrap items-center gap-x-6 gap-y-3 text-slate-500">
              <div className="flex items-center gap-2">
                <CheckCircle2 className="text-green-500 w-5 h-5 shrink-0" />
                <span className="text-sm font-medium">Suporte Presencial</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle2 className="text-green-500 w-5 h-5 shrink-0" />
                <span className="text-sm font-medium">Implantação Rápida</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle2 className="text-green-500 w-5 h-5 shrink-0" />
                <span className="text-sm font-medium">Sem contrato de fidelidade</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle2 className="text-green-500 w-5 h-5 shrink-0" />
                <span className="text-sm font-medium">Aqui você é único para nós</span>
              </div>
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, scale: 0.8 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ duration: 0.8, delay: 0.2 }}
            className="relative"
          >
            <div className="bg-sophos-card p-4 rounded-2xl shadow-2xl border border-slate-800 overflow-hidden max-w-5xl">
              {/* Slides */}
              <div className="relative">
                {dashboardSlides.map((slide, idx) => (
                  <motion.img
                    key={idx}
                    src={slide.src}
                    alt={slide.label}
                    referrerPolicy="no-referrer"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: idx === activeDashboard ? 1 : 0 }}
                    transition={{ duration: 0.6 }}
                    className={`rounded-xl w-full h-auto ${idx === activeDashboard ? 'block' : 'absolute inset-0'}`}
                  />
                ))}
              </div>

              {/* Dots — só exibe se tiver mais de 1 slide */}
              {dashboardSlides.length > 1 && (
                <div className="flex justify-center gap-2 mt-3">
                  {dashboardSlides.map((_, idx) => (
                    <button
                      key={idx}
                      onClick={() => goToDashboardSlide(idx)}
                      className={`h-1.5 rounded-full transition-all ${idx === activeDashboard ? 'bg-sophos-primary w-5' : 'bg-slate-600 w-1.5'
                        }`}
                    />
                  ))}
                </div>
              )}
            </div>
            {/* Floating elements */}
            <div className="absolute -bottom-6 -left-6 bg-sophos-card p-4 rounded-xl shadow-xl border border-slate-800 hidden sm:block">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-sophos-primary/20 rounded-lg text-sophos-primary">
                  <BarChart3 className="w-6 h-6" />
                </div>
                <div>
                  <p className="text-xs text-slate-500 font-bold uppercase tracking-wider">Vendas Hoje</p>
                  <p className="text-lg font-bold text-white">R$ 12.450,00</p>
                </div>
              </div>
            </div>
          </motion.div>
        </div>
      </section>

      {/* Segments Section */}
      <section id="segmentos" className="py-24 bg-sophos-bg">
        <div className="max-w-7xl mx-auto px-4">
          <div className="text-center mb-16">
            <h2 className="text-3xl lg:text-4xl font-bold text-white mb-4">Soluções para cada segmento</h2>
            <p className="text-slate-400 max-w-2xl mx-auto">Desenvolvemos funcionalidades específicas para atender as dores reais do seu dia a dia.</p>
          </div>
          <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
            {segments.map((seg, idx) => (
              <motion.div
                key={idx}
                whileHover={{ y: -10 }}
                className="p-8 rounded-2xl bg-sophos-card border border-slate-800 hover:border-sophos-primary/50 hover:shadow-xl transition-all"
              >
                <div className="w-16 h-16 bg-sophos-primary/10 text-sophos-primary rounded-2xl flex items-center justify-center mb-6">
                  {seg.icon}
                </div>
                <h3 className="text-xl font-bold mb-3 text-white">{seg.name}</h3>
                <p className="text-slate-400 leading-relaxed">{seg.desc}</p>
              </motion.div>
            ))}
          </div>
        </div>
      </section>

      {/* Clientes Section */}
      <section className="py-16 bg-sophos-bg overflow-hidden">
        <div className="max-w-7xl mx-auto px-4">
          <div className="text-center mb-12">
            <span className="inline-block py-1 px-3 rounded-full bg-sophos-primary/20 text-sophos-primary text-xs font-bold mb-4 tracking-wider uppercase">
              Quem já usa o Sophos
            </span>
            <h2 className="text-2xl lg:text-3xl font-bold text-white">Negócios que confiam em nós</h2>
          </div>

          {/* Desktop */}
          <div className="hidden md:flex justify-center flex-wrap gap-8">
            {clientes.map((cliente, idx) => (
              <motion.div
                key={idx}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5, delay: idx * 0.1 }}
                className="flex flex-col items-center gap-3"
              >
                <div className="w-28 h-28 rounded-2xl overflow-hidden border-2 border-slate-700 shadow-lg bg-sophos-card">
                  <img src={cliente.foto} alt={cliente.nome} className="w-full h-full object-cover" />
                </div>
                <div className="text-center">
                  <p className="text-white font-bold text-sm">{cliente.nome}</p>
                  <p className="text-slate-500 text-xs">{cliente.segmento}</p>
                </div>
              </motion.div>
            ))}

            {/* E muito mais */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5, delay: clientes.length * 0.1 }}
              className="flex flex-col items-center gap-3"
            >
              <div className="w-28 h-28 rounded-2xl border-2 border-dashed border-sophos-primary/50 bg-sophos-primary/10 flex items-center justify-center shadow-lg">
                <span className="text-sophos-primary font-extrabold text-3xl">+</span>
              </div>
              <div className="text-center">
                <p className="text-sophos-primary font-bold text-sm">E muito mais</p>
                <p className="text-slate-500 text-xs">Novos clientes todo mês</p>
              </div>
            </motion.div>
          </div>

          {/* Mobile: carrossel */}
          <div className="md:hidden flex flex-col items-center gap-6">
            <motion.div
              key={activeCliente}
              initial={{ opacity: 0, scale: 0.85 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ duration: 0.4 }}
              className="flex flex-col items-center gap-4"
            >
              <div className="w-36 h-36 rounded-2xl overflow-hidden border-2 border-slate-700 shadow-xl bg-sophos-card">
                <img src={clientes[activeCliente].foto} alt={clientes[activeCliente].nome} className="w-full h-full object-cover" />
              </div>
              <div className="text-center">
                <p className="text-white font-bold">{clientes[activeCliente].nome}</p>
                <p className="text-slate-500 text-sm">{clientes[activeCliente].segmento}</p>
              </div>
            </motion.div>

            <div className="flex gap-2">
              {clientes.map((_, idx) => (
                <button
                  key={idx}
                  onClick={() => {
                    setActiveCliente(idx);
                    if (intervalRef.current) clearInterval(intervalRef.current);
                    intervalRef.current = setInterval(() => {
                      setActiveCliente((prev) => (prev + 1) % clientes.length);
                    }, 3500);
                  }}
                  className={`h-1.5 rounded-full transition-all ${idx === activeCliente ? 'bg-sophos-primary w-5' : 'bg-slate-600 w-1.5'}`}
                />
              ))}
            </div>

            <p className="text-sophos-primary font-bold text-sm">+ E muito mais</p>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section id="recursos" className="py-24 bg-sophos-card text-white overflow-hidden relative">
        <div className="absolute top-0 left-0 w-full h-full opacity-10 pointer-events-none">
          <div className="absolute top-10 left-10 w-64 h-64 bg-sophos-primary rounded-full blur-3xl" />
          <div className="absolute bottom-10 right-10 w-96 h-96 bg-sophos-primary/50 rounded-full blur-3xl" />
        </div>

        <div className="max-w-7xl mx-auto px-4 relative z-10">
          <div className="grid lg:grid-cols-2 gap-16 items-center">
            <div>
              <h2 className="text-4xl lg:text-5xl font-bold mb-8 leading-tight">
                Tudo o que você precisa para <span className="text-sophos-primary">crescer</span> sem limites.
              </h2>
              <div className="grid sm:grid-cols-2 gap-8">
                {features.map((f, idx) => (
                  <div key={idx} className="space-y-4">
                    <div className="text-sophos-primary">
                      {f.icon}
                    </div>
                    <h4 className="text-xl font-bold">{f.title}</h4>
                    <p className="text-slate-400 text-sm leading-relaxed">{f.desc}</p>
                  </div>
                ))}
              </div>
            </div>
            <div className="lg:pl-12">
              <div className="space-y-6">
                <div className="bg-sophos-bg/50 p-6 rounded-2xl border border-slate-800 backdrop-blur-sm">
                  <h4 className="font-bold mb-2 flex items-center gap-2">
                    <CheckCircle2 className="text-sophos-primary w-5 h-5" />
                    Integração iFood & Delivery
                  </h4>
                  <p className="text-slate-400 text-sm">Receba pedidos automaticamente no seu PDV e envie para a cozinha sem redigitação.</p>
                </div>
                <div className="bg-sophos-bg/50 p-6 rounded-2xl border border-slate-800 backdrop-blur-sm">
                  <h4 className="font-bold mb-2 flex items-center gap-2">
                    <CheckCircle2 className="text-sophos-primary w-5 h-5" />
                    Emissão de NF-e e NFC-e
                  </h4>
                  <p className="text-slate-400 text-sm">Fiscal simplificado e automatizado para você ficar em dia com a legislação sem esforço.</p>
                </div>
                <div className="bg-sophos-bg/50 p-6 rounded-2xl border border-slate-800 backdrop-blur-sm">
                  <h4 className="font-bold mb-2 flex items-center gap-2">
                    <CheckCircle2 className="text-sophos-primary w-5 h-5" />
                    Implantações Personalizadas
                  </h4>
                  <p className="text-slate-400 text-sm">Precisando de algo que ainda não existe ? Implantamos para você !</p>
                </div>
                <div className="bg-sophos-bg/50 p-6 rounded-2xl border border-slate-800 backdrop-blur-sm">
                  <h4 className="font-bold mb-2 flex items-center gap-2">
                    <CheckCircle2 className="text-sophos-primary w-5 h-5" />
                    Relatorios Financeitos Avançados
                  </h4>
                  <p className="text-slate-400 text-sm">Relatórios detalhados e personalizados para uma melhor tomada de decisão financeira.</p>
                </div>
                <div className="bg-sophos-bg/50 p-6 rounded-2xl border border-slate-800 backdrop-blur-sm">
                  <h4 className="font-bold mb-2 flex items-center gap-2">
                    <CheckCircle2 className="text-sophos-primary w-5 h-5" />
                    Impressões Personalizadas
                  </h4>
                  <p className="text-slate-400 text-sm">Imprima recibos, comprovantes e documentos com o seu branding e identidade visual.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Integrations Section */}
      <section id="integracoes" className="py-24 bg-sophos-bg">
        <div className="max-w-7xl mx-auto px-4">
          <div className="text-center mb-16">
            <span className="inline-block py-1 px-3 rounded-full bg-sophos-primary/20 text-sophos-primary text-xs font-bold mb-4 tracking-wider uppercase">
              Ecossistema conectado
            </span>
            <h2 className="text-3xl lg:text-4xl font-bold text-white mb-4">Integrado com as ferramentas que você já usa</h2>
            <p className="text-slate-400 max-w-2xl mx-auto">Conectamos o Sophos ERP com os principais aplicativos do mercado para que você não precise mudar nada na sua operação.</p>
          </div>
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
            {/* WhatsApp */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-slate-800 bg-sophos-card hover:border-[#25D366]/50 transition-all cursor-default">
              <img src="images/empresas_integradas/whatsapp.jpg" alt="WhatsApp" className="w-14 h-14 rounded-2xl object-cover" />
              <span className="text-white font-semibold text-sm text-center">WhatsApp</span>
            </motion.div>
            {/* iFood */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-slate-800 bg-sophos-card hover:border-[#EA1D2C]/50 transition-all cursor-default">
              <img src="images/empresas_integradas/ifoodImagem.png" alt="iFood" className="w-14 h-14 rounded-2xl object-cover" />
              <span className="text-white font-semibold text-sm text-center">iFood</span>
            </motion.div>
            {/* 1 Delivery */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-slate-800 bg-sophos-card hover:border-[#F88113]/50 transition-all cursor-default">
              <img src="images/empresas_integradas/1DELIVERY.png" alt="1 Delivery" className="w-14 h-14 rounded-2xl object-contain" />
              <span className="text-white font-semibold text-sm text-center">1 Delivery</span>
            </motion.div>
            {/* Anota Aí */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-slate-800 bg-sophos-card hover:border-[#1E88E5]/50 transition-all cursor-default">
              <img src="images/empresas_integradas/ANOTAAI.png" alt="Anota Aí" className="w-14 h-14 rounded-2xl object-contain" />
              <span className="text-white font-semibold text-sm text-center">Anota Aí</span>
            </motion.div>
            {/* Cardápio Web */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-slate-800 bg-sophos-card hover:border-[#7C3AED]/50 transition-all cursor-default">
              <img src="images/empresas_integradas/cardapio_web_logo.jpg" alt="Cardápio Web" className="w-14 h-14 rounded-2xl object-cover" />
              <span className="text-white font-semibold text-sm text-center">Cardápio Web</span>
            </motion.div>
            {/* On Pedido */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-slate-800 bg-sophos-card hover:border-[#C0392B]/50 transition-all cursor-default">
              <img src="images/empresas_integradas/LogoOnPedido.png" alt="On Pedido" className="w-14 h-14 rounded-2xl object-contain" />
              <span className="text-white font-semibold text-sm text-center">On Pedido</span>
            </motion.div>
            {/* AiqFome */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-slate-800 bg-sophos-card hover:border-[#8B2FC9]/50 transition-all cursor-default">
              <img src="images/empresas_integradas/aiqfome_logo.jpg" alt="AiqFome" className="w-14 h-14 rounded-2xl object-cover" />
              <span className="text-white font-semibold text-sm text-center">AiqFome</span>
            </motion.div>
            {/* Juma */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-slate-800 bg-sophos-card hover:border-[#F5A623]/50 transition-all cursor-default">
              <img src="images/empresas_integradas/jumalogo.png" alt="Juma Entregas" className="w-14 h-14 rounded-2xl object-contain" />
              <span className="text-white font-semibold text-sm text-center">Juma Entregas</span>
            </motion.div>
            {/* Del Match */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-slate-800 bg-sophos-card hover:border-slate-500/50 transition-all cursor-default">
              <img src="images/empresas_integradas/delmatchlogo.png" alt="Del Match" className="w-14 h-14 rounded-2xl object-contain" />
              <span className="text-white font-semibold text-sm text-center">Del Match</span>
            </motion.div>
            {/* Otto */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-slate-800 bg-sophos-card hover:border-[#E74C3C]/50 transition-all cursor-default">
              <img src="images/empresas_integradas/ottologo.png" alt="Otto" className="w-14 h-14 rounded-2xl object-contain" />
              <span className="text-white font-semibold text-sm text-center">Otto</span>
            </motion.div>
            {/* Ápice Entregas */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-slate-800 bg-sophos-card hover:border-sophos-primary/50 transition-all cursor-default">
              <img src="images/empresas_integradas/LOGOAPICE.png" alt="Ápice Entregas" className="w-14 h-14 rounded-2xl object-contain" />
              <span className="text-white font-semibold text-sm text-center">Ápice Entregas</span>
            </motion.div>
            {/* Cardápio Digital Próprio */}
            <motion.div whileHover={{ y: -6, scale: 1.03 }} className="flex flex-col items-center gap-3 p-6 rounded-2xl border border-sophos-primary/40 bg-sophos-primary/10 hover:border-sophos-primary transition-all cursor-default">
              <div className="w-14 h-14 rounded-2xl bg-sophos-primary flex items-center justify-center">
                <img src="images/CEREBRO-VETOR.svg" alt="Cardápio Digital Sophos" className="w-9 h-9" />
              </div>
              <span className="text-sophos-primary font-bold text-sm text-center">Cardápio Digital Próprio</span>
            </motion.div>
          </div>
        </div>
      </section>

      {/* Cardápio Digital Próprio */}
      <section id="cardapio-digital" className="py-24 bg-sophos-card overflow-hidden">
        <div className="max-w-7xl mx-auto px-4">
          <div className="grid lg:grid-cols-2 gap-16 items-center">

            {/* Carrossel */}
            <motion.div
              initial={{ opacity: 0, x: -40 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
              className="flex justify-center"
            >
              {/* Phone mockup */}
              <div className="relative w-[280px] sm:w-[300px]">
                {/* Frame */}
                <div className="relative bg-slate-900 rounded-[3rem] border-[6px] border-slate-700 shadow-2xl overflow-hidden" style={{ boxShadow: '0 0 0 2px #0f172a, 0 30px 80px rgba(0,0,0,0.6)' }}>
                  {/* Notch */}
                  <div className="flex justify-center pt-3 pb-1 bg-slate-900">
                    <div className="w-20 h-5 bg-slate-800 rounded-full" />
                  </div>
                  {/* Screen */}
                  <div className="relative overflow-hidden bg-black" style={{ height: '560px' }}>
                    {cardapioSlides.map((slide, idx) => (
                      <motion.img
                        key={idx}
                        src={slide.src}
                        alt={`Cardápio Digital Sophos - tela ${idx + 1}`}
                        initial={{ opacity: 0 }}
                        animate={{ opacity: idx === activeCardapio ? 1 : 0 }}
                        transition={{ duration: 0.6 }}
                        className="absolute inset-0 w-full h-full object-cover object-top"
                      />
                    ))}
                  </div>
                  {/* Home bar */}
                  <div className="flex justify-center py-3 bg-slate-900">
                    <div className="w-24 h-1 bg-slate-600 rounded-full" />
                  </div>
                </div>
                {/* Side buttons */}
                <div className="absolute -right-[8px] top-24 w-[5px] h-12 bg-slate-700 rounded-r-lg" />
                <div className="absolute -left-[8px] top-20 w-[5px] h-8 bg-slate-700 rounded-l-lg" />
                <div className="absolute -left-[8px] top-32 w-[5px] h-8 bg-slate-700 rounded-l-lg" />
                {/* Dots */}
                <div className="flex justify-center gap-2 mt-6">
                  {cardapioSlides.map((_, idx) => (
                    <button
                      key={idx}
                      onClick={() => goToCardapioSlide(idx)}
                      className={`h-1.5 rounded-full transition-all ${idx === activeCardapio ? 'bg-sophos-primary w-5' : 'bg-slate-600 w-1.5'}`}
                    />
                  ))}
                </div>
              </div>
            </motion.div>

            {/* Texto */}
            <motion.div
              initial={{ opacity: 0, x: 40 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.6 }}
              className="space-y-8"
            >
              <div>
                <span className="inline-block py-1 px-3 rounded-full bg-sophos-primary/20 text-sophos-primary text-xs font-bold mb-4 tracking-wider uppercase">
                  Solução própria Sophos
                </span>
                <h2 className="text-4xl lg:text-5xl font-bold text-white leading-tight mb-4">
                  Cardápio Digital <span className="text-sophos-primary">completo</span> e integrado ao seu ERP
                </h2>
                <p className="text-slate-400 text-lg leading-relaxed">
                  Sem mensalidade extra, sem app de terceiros. O cardápio digital do Sophos já vem integrado ao seu sistema, com pedidos chegando direto na cozinha.
                </p>
              </div>

              <div className="space-y-5">
                <div className="flex items-start gap-4 p-5 rounded-2xl bg-sophos-bg border border-slate-800">
                  <div className="p-2.5 rounded-xl bg-sophos-primary/20 text-sophos-primary shrink-0">
                    <ShoppingBag className="w-6 h-6" />
                  </div>
                  <div>
                    <h4 className="text-white font-bold mb-1">Venda no Balcão</h4>
                    <p className="text-slate-400 text-sm leading-relaxed">Cliente escolhe pelo cardápio digital diretamente no balcão, agilizando o atendimento e reduzindo erros de pedido.</p>
                  </div>
                </div>

                <div className="flex items-start gap-4 p-5 rounded-2xl bg-sophos-bg border border-slate-800">
                  <div className="p-2.5 rounded-xl bg-sophos-primary/20 text-sophos-primary shrink-0">
                    <Smartphone className="w-6 h-6" />
                  </div>
                  <div>
                    <h4 className="text-white font-bold mb-1">Delivery Online</h4>
                    <p className="text-slate-400 text-sm leading-relaxed">Receba pedidos delivery direto no seu sistema, sem depender de plataformas externas ou pagar comissões abusivas.</p>
                  </div>
                </div>

                <div className="flex items-start gap-4 p-5 rounded-2xl bg-sophos-bg border border-slate-800">
                  <div className="p-2.5 rounded-xl bg-sophos-primary/20 text-sophos-primary shrink-0">
                    <ChefHat className="w-6 h-6" />
                  </div>
                  <div>
                    <h4 className="text-white font-bold mb-1">QR Code por Mesa</h4>
                    <p className="text-slate-400 text-sm leading-relaxed">O cliente escaneia o QR code da mesa, faz o pedido pelo celular e ele cai automaticamente na comanda e na cozinha.</p>
                  </div>
                </div>
              </div>
            </motion.div>
          </div>
        </div>
      </section>

      {/* Localização & Suporte Presencial */}
      <section className="py-24 bg-sophos-bg relative overflow-hidden">
        <div className="absolute inset-0 pointer-events-none">
          <div className="absolute -top-32 -left-32 w-96 h-96 bg-sophos-primary/5 rounded-full blur-3xl" />
          <div className="absolute -bottom-32 -right-32 w-96 h-96 bg-sophos-primary/5 rounded-full blur-3xl" />
        </div>
        <div className="max-w-7xl mx-auto px-4 relative z-10">
          <div className="text-center mb-16">
            <span className="inline-block py-1 px-3 rounded-full bg-sophos-primary/20 text-sophos-primary text-xs font-bold mb-4 tracking-wider uppercase">
              Onde estamos
            </span>
            <h2 className="text-3xl lg:text-5xl font-bold text-white mb-4">
              Atendemos você de <span className="text-sophos-primary">perto</span>
            </h2>
            <p className="text-slate-400 max-w-2xl mx-auto text-lg">
              Sediados em <strong className="text-white">São Carlos — SP</strong>, cobrimos toda a região e atendemos empresas em todo o Brasil.
            </p>
          </div>

          {/* Destaque suporte presencial */}
          <div className="bg-sophos-primary rounded-3xl p-10 lg:p-14 mb-12 relative overflow-hidden shadow-2xl">
            <div className="absolute top-0 right-0 w-80 h-80 bg-white/10 rounded-full -mr-40 -mt-40 blur-3xl" />
            <div className="absolute bottom-0 left-0 w-64 h-64 bg-black/10 rounded-full -ml-32 -mb-32 blur-2xl" />
            <div className="relative z-10 flex flex-col lg:flex-row items-center gap-10">
              <div className="flex-shrink-0 w-24 h-24 bg-white/20 rounded-3xl flex items-center justify-center shadow-xl">
                <Handshake className="w-14 h-14 text-white" />
              </div>
              <div className="text-center lg:text-left">
                <p className="text-white/70 font-bold uppercase tracking-widest text-sm mb-2">O que nos diferencia</p>
                <h3 className="text-3xl lg:text-4xl font-extrabold text-white mb-3">
                  Suporte 100% Presencial
                </h3>
                <p className="text-white/80 text-lg leading-relaxed max-w-2xl">
                  Quando você precisa, um especialista Sophos vai até a sua empresa. Sem chamados sem resposta, sem atendimento robô — <strong className="text-white">gente de verdade resolvendo o seu problema na hora.</strong>
                </p>
              </div>
            </div>
          </div>

          {/* Cards de cobertura */}
          <div className="grid md:grid-cols-3 gap-6">
            <motion.div
              whileHover={{ y: -6 }}
              className="bg-sophos-card border border-slate-800 hover:border-sophos-primary/40 rounded-2xl p-8 flex flex-col items-center text-center gap-4 transition-all"
            >
              <div className="w-14 h-14 rounded-2xl bg-sophos-primary/20 flex items-center justify-center text-sophos-primary">
                <MapPin className="w-7 h-7" />
              </div>
              <h4 className="text-white font-bold text-xl">São Carlos — SP</h4>
              <p className="text-slate-400 text-sm leading-relaxed">Nossa sede. Implantação, treinamento e suporte presencial para clientes da cidade e arredores.</p>
            </motion.div>

            <motion.div
              whileHover={{ y: -6 }}
              className="bg-sophos-card border border-slate-800 hover:border-sophos-primary/40 rounded-2xl p-8 flex flex-col items-center text-center gap-4 transition-all"
            >
              <div className="w-14 h-14 rounded-2xl bg-sophos-primary/20 flex items-center justify-center text-sophos-primary">
                <Users className="w-7 h-7" />
              </div>
              <h4 className="text-white font-bold text-xl">Região de São Carlos</h4>
              <p className="text-slate-400 text-sm leading-relaxed">Araraquara, Ribeirão Preto, Campinas, Piracicaba e toda a região central paulista com atendimento presencial.</p>
            </motion.div>

            <motion.div
              whileHover={{ y: -6 }}
              className="bg-sophos-card border border-slate-800 hover:border-sophos-primary/40 rounded-2xl p-8 flex flex-col items-center text-center gap-4 transition-all"
            >
              <div className="w-14 h-14 rounded-2xl bg-sophos-primary/20 flex items-center justify-center text-sophos-primary">
                <Globe className="w-7 h-7" />
              </div>
              <h4 className="text-white font-bold text-xl">Todo o Brasil</h4>
              <p className="text-slate-400 text-sm leading-relaxed">Atendemos empresas em qualquer estado com suporte remoto ágil e visitas presenciais mediante agendamento.</p>
            </motion.div>
          </div>
        </div>
      </section>

      {/* Testimonials / Trust */}
      <section id="sobre" className="py-24 bg-sophos-bg">
        <div className="max-w-7xl mx-auto px-4">
          <div className="bg-sophos-primary rounded-[40px] p-12 lg:p-20 text-white relative overflow-hidden shadow-2xl">
            <div className="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full -mr-32 -mt-32 blur-2xl" />
            <div className="relative z-10 grid lg:grid-cols-2 gap-12 items-center">
              <div className="bg-sophos-card p-8 rounded-3xl text-white shadow-xl border border-white/10">
                <h3 className="text-2xl font-bold mb-6">Solicite contato</h3>
                {formStatus === 'success' ? (
                  <div className="flex flex-col items-center gap-4 py-8 text-center">
                    <CheckCircle2 className="w-14 h-14 text-green-400" />
                    <p className="text-lg font-semibold text-white">{formMessage}</p>
                    <p className="text-slate-400 text-sm">Verifique seu e-mail. Nossa equipe entrará em contato em breve!</p>
                    <button onClick={() => setFormStatus('idle')} className="mt-2 text-sophos-primary underline text-sm">
                      Enviar outra solicitação
                    </button>
                  </div>
                ) : (
                  <form className="space-y-4" onSubmit={handleSubmit}>
                    <div>
                      <label className="text-xs font-bold uppercase tracking-wider text-slate-500 mb-1 block">Nome da Empresa</label>
                      <input
                        type="text"
                        required
                        value={empresa}
                        onChange={(e) => setEmpresa(e.target.value)}
                        className="w-full bg-sophos-bg border border-slate-800 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-sophos-primary transition-all text-white"
                        placeholder="Ex: Restaurante do João"
                      />
                    </div>
                    <div>
                      <label className="text-xs font-bold uppercase tracking-wider text-slate-500 mb-1 block">Seu E-mail</label>
                      <input
                        type="email"
                        required
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        className="w-full bg-sophos-bg border border-slate-800 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-sophos-primary transition-all text-white"
                        placeholder="joao@email.com"
                      />
                    </div>
                    <div>
                      <label className="text-xs font-bold uppercase tracking-wider text-slate-500 mb-1 block">WhatsApp</label>
                      <input
                        type="tel"
                        required
                        value={whatsapp}
                        onChange={(e) => setWhatsapp(e.target.value)}
                        className="w-full bg-sophos-bg border border-slate-800 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-sophos-primary transition-all text-white"
                        placeholder="(00) 00000-0000"
                      />
                    </div>
                    {formStatus === 'error' && (
                      <p className="text-red-400 text-sm text-center">{formMessage}</p>
                    )}
                    <button
                      type="submit"
                      disabled={formStatus === 'loading'}
                      className="w-full bg-sophos-primary text-white py-4 rounded-xl font-bold text-lg hover:opacity-90 transition-all shadow-lg shadow-sophos-primary/20 disabled:opacity-60 disabled:cursor-not-allowed"
                    >
                      {formStatus === 'loading' ? 'Enviando...' : 'Enviar Solicitação'}
                    </button>
                  </form>
                )}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-sophos-card border-t border-slate-800 pt-20 pb-10">
        <div className="max-w-7xl mx-auto px-4 grid md:grid-cols-2 lg:grid-cols-4 gap-12 mb-16">
          <div className="space-y-6">
            <div className="flex items-center gap-2">
              <img
                width={30}
                height={30}
                src="images/CEREBRO-VETOR.svg"
                alt="Dashboard do Sophos ERP"
                className="rounded-xl opacity-100"
                referrerPolicy="no-referrer"
              />
              <span className="text-xl font-bold tracking-tight text-white">Sophos</span>
            </div>
            <p className="text-slate-400 text-sm leading-relaxed">
              Especialistas em desenvolver soluções tecnológicas que simplificam a vida do empreendedor no setor alimentício e varejo.
            </p>
          </div>

          <div>
            <h4 className="font-bold mb-6 text-white">Soluções</h4>
            <ul className="space-y-4 text-slate-400 text-sm">
              <li><a href="#" className="hover:text-sophos-primary transition-colors">ERP para Restaurantes</a></li>
              <li><a href="#" className="hover:text-sophos-primary transition-colors">ERP para Mercados</a></li>
              <li><a href="#" className="hover:text-sophos-primary transition-colors">PDV e Frente de Caixa</a></li>
              <li><a href="#" className="hover:text-sophos-primary transition-colors">Controle de Estoque</a></li>
            </ul>
          </div>

        </div>

        <div className="max-w-7xl mx-auto px-4 pt-8 border-t border-slate-800 text-center text-slate-500 text-xs">
          <p>© 2026 Sophos Aplicativos e Tecnologia. Todos os direitos reservados.</p>
        </div>
      </footer>
    </div>
  );
}
