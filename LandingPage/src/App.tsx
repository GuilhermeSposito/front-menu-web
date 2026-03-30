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
  X
} from "lucide-react";
import { useState, useEffect, useRef } from "react";

type FormStatus = 'idle' | 'loading' | 'success' | 'error';

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

    try {
      const res = await fetch('/api/orcamento', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ empresa, email, whatsapp }),
      });

      const data = await res.json();

      if (res.ok) {
        setFormStatus('success');
        setFormMessage(data.message || 'Solicitação enviada com sucesso!');
        setEmpresa('');
        setEmail('');
        setWhatsapp('');
      } else {
        setFormStatus('error');
        setFormMessage(data.error || 'Erro ao enviar. Tente novamente.');
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
    {
      nome: "Restaurante Piassa",
      segmento: "Restaurante",
      // Substitua pelo caminho da foto real: ex: "images/clientes/piassa.jpg"
      foto: null,
      iniciais: "RP",
      cor: "#F88113",
    },
    {
      nome: "Pastel da Li",
      segmento: "Lanchonete",
      // Substitua pelo caminho da foto real: ex: "images/clientes/pastel-da-li.jpg"
      foto: null,
      iniciais: "PL",
      cor: "#0ea5e9",
    },
    {
      nome: "Burgues on the Table",
      segmento: "Hamburgueria",
      // Substitua pelo caminho da foto real: ex: "images/clientes/burgues.jpg"
      foto: null,
      iniciais: "BT",
      cor: "#22c55e",
    },
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
            <span className="inline-block py-1 px-3 rounded-full bg-sophos-primary/20 text-sophos-primary text-sm font-bold mb-6 tracking-wider uppercase text-center">
              Sistema ERP para Restaurantes, Bares, Mercados e Conveniências
            </span>
            <h1 className="text-5xl lg:text-7xl font-extrabold text-white leading-[1.1] mb-8">
              Esta Cansado de sistemas <span className="text-sophos-primary">grandes</span> que não te dão suporte e atenção ?
            </h1>
            <p className="text-xl text-slate-400 mb-10 leading-relaxed max-w-lg">
              O Sophos ERP é a sua solução completa para gestão de restaurantes, bares, mercados e conveniências.
              Com Atendimento Personalizado, Suporte 24/7 e Implantação Rápida, estamos prontos para transformar a gestão do seu negócio.
              <span className="text-sophos-primary"> Aqui você é unico para nós !</span>
            </p>
            <div className="flex flex-col sm:flex-row gap-4">
              <button onClick={() => window.open("https://wa.me/5516992366175", "_blank")} className="bg-sophos-primary text-white px-8 py-4 rounded-full font-bold text-lg hover:opacity-90 transition-all flex items-center justify-center gap-2 shadow-xl shadow-sophos-primary/20 group">
                Solicitar Demonstração
                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
              </button>
            </div>

            <div className="mt-12 flex items-center gap-6 text-slate-500">
              <div className="flex items-center gap-2">
                <CheckCircle2 className="text-green-500 w-5 h-5" />
                <span className="text-sm font-medium">Suporte 24/7</span>
              </div>
              <div className="flex items-center gap-2">
                <CheckCircle2 className="text-green-500 w-5 h-5" />
                <span className="text-sm font-medium">Implantação Rápida</span>
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

          {/* Desktop: todos visíveis lado a lado */}
          <div className="hidden md:flex justify-center gap-12">
            {clientes.map((cliente, idx) => (
              <motion.div
                key={idx}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5, delay: idx * 0.15 }}
                className="flex flex-col items-center gap-4"
              >
                <div
                  className="w-28 h-28 rounded-full flex items-center justify-center text-2xl font-extrabold text-white shadow-lg border-4 border-sophos-card overflow-hidden"
                  style={{ background: cliente.foto ? 'transparent' : cliente.cor }}
                >
                  {cliente.foto ? (
                    <img src={cliente.foto} alt={cliente.nome} className="w-full h-full object-cover" />
                  ) : (
                    cliente.iniciais
                  )}
                </div>
                <div className="text-center">
                  <p className="text-white font-bold text-sm">{cliente.nome}</p>
                  <p className="text-slate-500 text-xs">{cliente.segmento}</p>
                </div>
              </motion.div>
            ))}
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
              <div
                className="w-32 h-32 rounded-full flex items-center justify-center text-3xl font-extrabold text-white shadow-xl border-4 border-sophos-card overflow-hidden"
                style={{ background: clientes[activeCliente].foto ? 'transparent' : clientes[activeCliente].cor }}
              >
                {clientes[activeCliente].foto ? (
                  <img src={clientes[activeCliente].foto!} alt={clientes[activeCliente].nome} className="w-full h-full object-cover" />
                ) : (
                  clientes[activeCliente].iniciais
                )}
              </div>
              <div className="text-center">
                <p className="text-white font-bold">{clientes[activeCliente].nome}</p>
                <p className="text-slate-500 text-sm">{clientes[activeCliente].segmento}</p>
              </div>
            </motion.div>

            {/* Dots */}
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
                  className={`w-2 h-2 rounded-full transition-all ${idx === activeCliente ? 'bg-sophos-primary w-5' : 'bg-slate-600'
                    }`}
                />
              ))}
            </div>
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
